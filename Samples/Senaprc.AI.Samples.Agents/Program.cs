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
using Senaprc.AI.Samples.Agents;
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

AgentKeys.AgentKey1 = config.GetSection("AgentKey1").Value;
AgentKeys.AgentKey2 = config.GetSection("AgentKey2").Value;
AgentKeys.AgentKey3 = config.GetSection("AgentKey3").Value;

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

// Create the Administrtor
var administrator = new SemanticKernelAgent(
    kernel: kernel,
    name: "行政主管",
    systemMessage: """
    你是行政主管，你拥有制定合同、确认项目在公司层面的风险等责任。你正在处理一个项目需求的梳理，并且需要答复客户可行性，以及最终可能的合同条款。
    你可以向你的下属提问，以获得你想要的答案，并按照要求总结后进行回复。

    请注意：每一项决策都必须至少通过产品经理、项目经理的反馈和确认，才能最终给出最终的回复。

    这些是你的下属:
    - 产品经理：产品经理负责产品的功能设计和产品的功能规划。
    - 项目经理: 项目经理负责所有项目的开发任务的安排和功能可行性的评估。
    """)
    .RegisterTextMessageConnector()
    .RegisterPrintWechatMessage();

//iWantToRun.ImportPluginFromObject(webSearchPlugin);

// Create the Product Manager
var productManager = new SemanticKernelAgent(
       kernel: kernel,
       name: "产品经理",
       systemMessage: """
       你是产品经理，你向行政主管负责，并且负责回答他的所有问题。

       为了确保你有最新的信息，你可以使用网络搜索插件在回答问题之前在网上搜索信息，也可以根据你的经验，按照要求回答问题，并作出相对应的规划和决策。
       """)
    .RegisterTextMessageConnector()
    .RegisterPrintWechatMessage();

// Create the Project Manager
var projectManager = new SemanticKernelAgent(
       kernel: kernel,
       name: "项目经理",
       systemMessage: """
       你是项目经理，你向行政主管负责，并且负责回答他的所有问题。
       产品经理可能会告诉你项目的规划方案，此时你需要对其进行评估，并安排对应的开发任务。

       为了确保你有最新的信息，你可以使用网络搜索插件在回答问题之前在网上搜索信息，也可以根据你的经验，按照要求回答问题，并作出相对应的规划和决策。

       下面是你在当前项目中的下属，可以进行任务的开发，你的任务安排中需要包含人名和具体的任务安排，任务需要细分到具体的开发内容而不仅仅是功能点：
       - 前端开发工程师：Light、Damon、Karl
       - 后端开发工程师：Adens、Dylan
       - 测试工程师：Amy
       - 设计师：Ida
       """)
    .RegisterTextMessageConnector()
    .RegisterPrintWechatMessage();


// Create the hearing member
var hearingMember = new UserProxyAgent(name: "BA");

// Create the group admin
var admin = new SemanticKernelAgent(
    kernel: kernel,
    name: "admin",
    systemMessage: "你是群管理员.")
    .RegisterTextMessageConnector();

// Create the AI team
// define the transition among group members
// we only allow the following transitions:
// hearingMember -> 行政
// 行政 -> 项目经理
// 行政 -> 产品经理
// 项目经理 -> 行政
// 产品经理 -> 行政
// 行政 -> hearingMember

var hearingMember2Administrator = Transition.Create(hearingMember, administrator);

var admin2ProjectManager = Transition.Create(administrator, projectManager);
var admin2ProductManager = Transition.Create(administrator, productManager);
var projectManager2Administrator = Transition.Create(projectManager, administrator);
var productManager2Administrator = Transition.Create(productManager, administrator);

var administrator2HearingMember = Transition.Create(administrator, hearingMember);

var graph = new Graph([hearingMember2Administrator,
    admin2ProjectManager,
    admin2ProductManager,
    projectManager2Administrator,
    productManager2Administrator,
    administrator2HearingMember]);
var aiTeam = new GroupChat(
    members: [hearingMember, administrator, productManager, projectManager],
    admin: admin,
    workflow: graph);

// start the chat
// generate a greeting message to hearing member from Administrator
var greetingMessage = await administrator.SendAsync("你好，如果已经就绪，请告诉我们“已就位”，并和 BA 打个招呼");
await administrator.SendMessageToGroupAsync(
    groupChat: aiTeam,
    chatHistory: [greetingMessage],
    maxRound: 20);

