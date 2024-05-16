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

//var openaiAPI = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("OPENAI_API_KEY is not set");

var configBuilder = new ConfigurationBuilder();
var appsettingsJsonFileName = SampleHelper.GetAppSettingsFile();
configBuilder.AddJsonFile(appsettingsJsonFileName, false, false);
Console.WriteLine("完成 appsettings.json 添加");
var config = configBuilder.Build();
Console.WriteLine("完成 ServiceCollection 和 ConfigurationBuilder 初始化");

var setting = (SenparcAiSetting)Senparc.AI.Config.SenparcAiSetting;//也可以留空，将自动获取

var _semanticAiHandler = new SemanticAiHandler(null);

var bingSearchAPIKey = "";// Environment.GetEnvironmentVariable("BING_API_KEY") ?? throw new Exception("BING_API_KEY is not set");""

var gpt3_5 = "gpt-3.5-turbo";
var gpt_4 = "gpt-4-32k";// "gpt-4-turbo";

// Create the CEO
var ceo = new OpenAIChatAgent(
    openAIClient: openaiClient,
    name: "Elon Musk",
    modelName: gpt_4,
    systemMessage: """
    You are Elon Musk, CEO of Tesla. You are in a hearing about Tesla.
    When a question about tesla is asked, You can ask your subordinates to answer the question.

    Here are your subordinates:
    - cmo: Chief Marketing Officer who is responsible for answering all market-related questions.
    """)
    .RegisterMessageConnector()
    .RegisterPrintMessage();

// Create the cmo
//var kernelBuilder = Kernel.CreateBuilder()
//    .AddAzureOpenAIChatCompletion(gpt_4, openaiClient, null);
//.AddOpenAIChatCompletion(modelId: gpt3_5, apiKey: openaiAPI);
var bingSearch = new BingConnector(bingSearchAPIKey);
var webSearchPlugin = new WebSearchEnginePlugin(bingSearch);

var iWantToRun = _semanticAiHandler.IWantTo(setting)
                .ConfigModel(ConfigModel.Chat, "JeffreySu")
                .BuildKernel();

iWantToRun.Kernel.Plugins.AddFromObject(webSearchPlugin);
var kernel = kernelBuilder.Build();


//更多绑定操作参见：https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2
var senparcSetting = new SenparcSetting();
config.GetSection("SenparcSetting").Bind(senparcSetting);

var services = new ServiceCollection();

services.AddSenparcGlobalServices(config)
        .AddSenparcAI(config);


var parameter = new PromptConfigParameter()
{
    MaxTokens = 2000,
    Temperature = 0.7,
    TopP = 0.5,
};

//await Console.Out.WriteLineAsync(localResponse);
//var remoteResponse = await huggingFaceRemote.CompleteAsync(Input);
// modelName: "gpt-4-32k"*/


var bingSearch2 = new BingConnector(bingSearchAPIKey);
var webSearchPlugin2 = new WebSearchEnginePlugin(bingSearch);

var tempKernel = _semanticAiHandler.IWantTo(setting)
                .ConfigModel(ConfigModel.Chat, "JeffreySu");
tempKernel.Kernel.Plugins.AddFromObject(webSearchPlugin2);
var senparcAIKernel = tempKernel.BuildKernel();

var kernel2 = _semanticAiHandler.SemanticKernelHelper.GetKernel();

var cmo = new SemanticKernelAgent(
       kernel: kernel2,//kernel
       name: "cmo",
       systemMessage: """
       You are cmo, you report to Elon and you answer all market-related question.

       To make sure you have the most up-to-date information, you can use the web search plugin to search for information on the web before answering questions.
       """)
    .RegisterMessageConnector()
    .RegisterPrintMessage();

// Create the hearing member
var hearingMember = new UserProxyAgent(name: "hearingMember");

// Create the group admin
var admin = new OpenAIChatAgent(
    openAIClient: openaiClient,
    name: "admin",
    modelName: gpt_4,
    systemMessage: "You are the group admin.")
    .RegisterMessageConnector();

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

