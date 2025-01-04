// See https://aka.ms/new-console-template for more information
using System.Net.Mail;
using Senaprc.AI.Samples.Agents;

Console.WriteLine("欢迎来到 Senparc.AI Agent 测试项目，点击任意键继续");
Console.ReadKey();

Console.WriteLine("开始注册所有 Agents");
AgentHelper.RegisterAllAgents();
Console.WriteLine("注册完毕");

Console.WriteLine("请输入你的需求");
var request = Console.ReadLine();

//完成现阶段计划
AgentHelper.AgentCollection["Guide"].Execute(new Dictionary<string, string> {  { "INPUT" , request } });