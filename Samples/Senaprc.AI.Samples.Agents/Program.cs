// See https://aka.ms/new-console-template for more information

using AutoGen;
using AutoGen.Core;
using AutoGen.Mistral;
using AutoGen.SemanticKernel;
using AutoGen.SemanticKernel.Extension;
using Google.Rpc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Senparc.AI;
using Senparc.AI.Agents;
using Senparc.AI.Agents.AgentExtensions;
using Senparc.AI.Agents.AgentUtility;
using Senparc.AI.Entities;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Samples.Agents;
using Senparc.CO2NET;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.RegisterServices;


var configBuilder = new ConfigurationBuilder();
var appsettingsJsonFileName = SampleHelper.GetAppSettingsFile();
configBuilder.AddJsonFile(appsettingsJsonFileName, false, false);

Console.WriteLine("appsettings.json added");
var config = configBuilder.Build();

#region Initialize Senparc foundation

var senparcSetting = new SenparcSetting();
config.GetSection("SenparcSetting").Bind(senparcSetting);

var services = new ServiceCollection();

services.AddSenparcGlobalServices(config)
        .AddSenparcAI(config);

Console.WriteLine("ServiceCollection and ConfigurationBuilder initialized");
#endregion

#region Initialize model configuration

var setting = (SenparcAiSetting)Senparc.AI.Config.SenparcAiSetting;//can also be left empty; it will be obtained automatically

var _semanticAiHandler = new SemanticAiHandler(setting);

var parameter = new PromptConfigParameter()
{
    MaxTokens = 2000,
    Temperature = 0,
    TopP = 0.5,
};

var iWantToRunConfig = _semanticAiHandler.IWantTo(setting)
                .ConfigModel(ConfigModel.Chat, "JeffreySu");


var bingSearchAPIKey = config.GetSection("BingSearchAPIKey").Value;

var bingSearch = new BingConnector(bingSearchAPIKey);
var webSearchPlugin = new WebSearchEnginePlugin(bingSearch);
iWantToRunConfig.Kernel.Plugins.AddFromObject(webSearchPlugin);

var iWantToRun = iWantToRunConfig.BuildKernel();
var kernel = iWantToRun.Kernel;

#endregion

#region AgentKey for WeCom

AgentKeys.AgentKey1 = config.GetSection("AgentKey1").Value;
AgentKeys.AgentKey2 = config.GetSection("AgentKey2").Value;
AgentKeys.AgentKey3 = config.GetSection("AgentKey3").Value;

#endregion

var accessor = new SemanticKernelAgent(
    kernel: kernel,
    name: "Accessor",
    systemMessage: """
    You are the coordinator for the whole conversation. You pass messages and receive the final result.
    """)
    .RegisterTextMessageConnector();

// Create the Administrtor
var ceo = new SemanticKernelAgent(
    kernel: kernel,
    name: "CEO",
    systemMessage: """
    You are the company CEO. You are responsible for drafting contracts and confirming company-level project risks. You are analyzing project requirements and need to respond to the customer about feasibility and possible contract terms.
    You can ask your subordinates questions to get the answers you need, then summarize and reply as requested.

    Note: each decision must receive feedback and confirmation from at least the Product Manager and Project Manager before the final response is produced.

    These are your subordinates. You can ask them questions with @:
    - Product Manager:Responsible for product feature design and product planning.
    - Project Manager: Responsible for arranging development tasks and evaluating feature feasibility for all projects.

    e.g: @Product Manager xxxx.

    Your customer is John Doe.
    """)
    .RegisterTextMessageConnector()
    .RegisterCustomPrintMessage(new PrintWechatMessageMiddleware(AgentKeys.SendWechatMessage));

// Create the Product Manager
var productManager = new SemanticKernelAgent(
       kernel: kernel,
       name: "Product Manager",
       systemMessage: """
       You are the Product Manager. You report to the CEO and answer all CEO questions.

       To ensure you have the latest information, you may use the web search plugin before answering, or answer from your experience as requested and make the corresponding plans and decisions.
       """)
    .RegisterTextMessageConnector()
    .RegisterCustomPrintMessage(new PrintWechatMessageMiddleware(AgentKeys.SendWechatMessage));

// Create the Project Manager
var projectManager = new SemanticKernelAgent(
       kernel: kernel,
       name: "Project Manager",
       systemMessage: """
       You are the Project Manager. You report to the CEO and answer all CEO questions.
       The Product Manager may describe the project plan. Evaluate it and arrange the corresponding development tasks.

       To ensure you have the latest information, you may use the web search plugin before answering, or answer from your experience as requested and make the corresponding plans and decisions.

       When arranging project development tasks, include each person's name and specific task assignment. Break tasks down into concrete development work, not just feature points.
       """)
    .RegisterTextMessageConnector()
    .RegisterCustomPrintMessage(new PrintWechatMessageMiddleware(AgentKeys.SendWechatMessage));


// Create the hearing member
var user = new UserProxyAgent(name: "John Doe")
    .RegisterPrintMessage();

// Create the group admin
var admin = new SemanticKernelAgent(
    kernel: kernel,
    name: "admin")
    .RegisterMessageConnector()
    .RegisterPrintMessage();

// Create the AI team
// define the transition among group members
// we only allow the following transitions:
// hearingMember -> Admin
// Admin -> Project Manager
// Admin -> Product Manager
// Project Manager -> Admin
// Product Manager -> Admin
// Admin -> hearingMember

#region Old method

//var hearingMember2Administrator = Transition.Create(hearingMember, administrator);

//var admin2ProjectManager = Transition.Create(administrator, projectManager);
//var admin2ProductManager = Transition.Create(administrator, productManager);
//var projectManager2Administrator = Transition.Create(projectManager, administrator);
//var productManager2Administrator = Transition.Create(productManager, administrator);

//var administrator2HearingMember = Transition.Create(administrator, hearingMember);

//var graph = new Graph([hearingMember2Administrator,
//    admin2ProjectManager,
//    admin2ProductManager,
//    projectManager2Administrator,
//    productManager2Administrator,
//    administrator2HearingMember]);

//var aiTeam = new GroupChat(
//    members: graphConnect.Agents.Values,
//    admin: admin,
//    workflow: graphConnect.Graph);

#endregion

var graphConnector = GraphBuilder
    .Start()
    .ConnectFrom(user).TwoWay(ceo)//.AlsoTwoWay(accessor)
    .ConnectFrom(ceo).TwoWay(projectManager).AlsoTwoWay(productManager)//.AlsoTwoWay(accessor)
    .Finish();

var aiTeam = graphConnector.CreateAiTeam(admin);

Start:
Console.WriteLine();
Console.WriteLine("Senparc.AI Sample Agent started");
Console.WriteLine("Open-source repository:https://github.com/Senparc/Senparc.AI");
Console.WriteLine("-----------------------");
await Console.Out.WriteLineAsync("Enter exit at any time to leave the conversation and restart.");
Console.WriteLine("-----------------------");
Console.WriteLine("Waiting for Agent response...");
Console.WriteLine("-----------------------");

// start the chat
// generate a greeting message to hearing member from Administrator
var greetingMessage = await ceo.SendAsync("Hello. If you are ready, tell us 'ready' and greet John Doe.");

try
{
    await foreach (var message in aiTeam.SendAsync(chatHistory: [greetingMessage],
          maxRound: 20))
    {
        // process exit
        if (message.GetContent()?.Contains("exit") is true)
        {
            Console.WriteLine("You have exited the conversation");
            return;
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Sorry, an exception occurred: {ex.Message}. ");
}



Console.WriteLine("This conversation round has ended. Let us restart.");
Console.WriteLine();

goto Start;
