// See https://aka.ms/new-console-template for more information

using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using AutoGen.SemanticKernel;
using AutoGen.SemanticKernel.Extension;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Senparc.AI;
using Senparc.AI.Entities;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Samples.Agents;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;


var configBuilder = new ConfigurationBuilder();
var appsettingsJsonFileName = SampleHelper.GetAppSettingsFile();
configBuilder.AddJsonFile(appsettingsJsonFileName, false, false);

Console.WriteLine("完成 appsettings.json 添加");
var config = configBuilder.Build();

var senparcSetting = new SenparcSetting();
config.GetSection("SenparcSetting").Bind(senparcSetting);

var services = new ServiceCollection();

services.AddSenparcGlobalServices(config)
        .AddSenparcAI(config);


Console.WriteLine("完成 ServiceCollection 和 ConfigurationBuilder 初始化");

var setting = (SenparcAiSetting)Senparc.AI.Config.SenparcAiSetting;//也可以留空，将自动获取

var _semanticAiHandler = new SemanticAiHandler(setting);

var bingSearchAPIKey = config.GetSection("BingSearchAPIKey").Value;

var parameter = new PromptConfigParameter()
{
    MaxTokens = 2000,
    Temperature = 0.7,
    TopP = 0.5,
};

var iWantToRunConfig = _semanticAiHandler.IWantTo(setting)
                .ConfigModel(ConfigModel.Chat, "JeffreySu");

var bingSearch = new BingConnector(bingSearchAPIKey);
var webSearchPlugin = new WebSearchEnginePlugin(bingSearch);
iWantToRunConfig.Kernel.Plugins.AddFromObject(webSearchPlugin);

var iWantToRun = iWantToRunConfig.BuildKernel();
var kernel = iWantToRun.Kernel;

// Create the CEO
var ceo = new SemanticKernelAgent(
    kernel: kernel,
    name: "Elon Musk",
    systemMessage: """
    You are Elon Musk, CEO of Tesla. You are in a hearing about Tesla.
    When a question about tesla is asked, You can ask your subordinates to answer the question.

    Here are your subordinates:
    - cmo: Chief Marketing Officer who is responsible for answering all market-related questions.
    """)
    .RegisterTextMessageConnector()
    .RegisterPrintWechatMessage();

//iWantToRun.ImportPluginFromObject(webSearchPlugin);

// Create the CMO
var cmo = new SemanticKernelAgent(
       kernel: kernel,
       name: "cmo",
       systemMessage: """
       You are cmo, you report to Elon and you answer all market-related question.

       To make sure you have the most up-to-date information, you can use the web search plugin to search for information on the web before answering questions.
       """)
    .RegisterTextMessageConnector()
    .RegisterPrintWechatMessage();

// Create the hearing member
var hearingMember = new UserProxyAgent(name: "hearingMember");

// Create the group admin
var admin = new SemanticKernelAgent(
    kernel: kernel,
    name: "admin",
    systemMessage: "You are the group admin.")
    .RegisterTextMessageConnector();

// Create the AI team
// define the transition among group members
// we only allow the following transitions:
// hearingMember -> ceo
// ceo -> cmo
// cmo -> ceo
// ceo -> hearingMember

var hearingMember2Ceo = Transition.Create(hearingMember, ceo);
var ceo2Ds = Transition.Create(ceo, cmo);
var ds2Ceo = Transition.Create(cmo, ceo);
var ceo2HearingMember = Transition.Create(ceo, hearingMember);

var graph = new Graph([hearingMember2Ceo, ceo2Ds, ds2Ceo, ceo2HearingMember]);
var aiTeam = new GroupChat(
    members: [hearingMember, ceo, cmo],
    admin: admin,
    workflow: graph);

// start the chat
// generate a greeting message to hearing member from ceo
var greetingMessage = await ceo.SendAsync("generate a greeting message to hearing memeber");
await ceo.SendMessageToGroupAsync(
    groupChat: aiTeam,
    chatHistory: [greetingMessage],
    maxRound: 20);

