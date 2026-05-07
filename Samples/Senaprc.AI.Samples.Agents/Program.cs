using AutoGen.Core;
using AutoGen.SemanticKernel;
using Senparc.AI.Agents.AgentExtensions;

var workflow = GraphBuilder.Start();

var ceo = new SemanticKernelAgent("CEO");
var pm = new SemanticKernelAgent("产品经理");
var rd = new SemanticKernelAgent("项目经理");

var graph = workflow
    .ConnectFrom(ceo).TwoWay(pm).AlsoTwoWay(rd)
    .Finish();

var team = graph.CreateAiTeam(ceo);
Console.WriteLine($"[调试] 团队成员数量：{team.Members.Count}");

var connector = ceo.RegisterTextMessageConnector();
var message = await connector.Agent.GenerateReplyAsync([new TextMessage("User", "请输出：团队已就位。")]);
Console.WriteLine(message.FormatMessage());