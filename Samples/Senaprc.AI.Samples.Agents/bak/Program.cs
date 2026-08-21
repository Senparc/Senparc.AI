// See https://aka.ms/new-console-template for more information
using System.Net.Mail;
using Senaprc.AI.Samples.Agents;

Console.WriteLine("Welcome to the Senparc.AI Agent test project. Press any key to continue.");
Console.ReadKey();

Console.WriteLine("Start registering all Agents");
AgentHelper.RegisterAllAgents();
Console.WriteLine("Registration completed");

Console.WriteLine("Enter your requirement");
var request = Console.ReadLine();

//Complete the current-stage plan
AgentHelper.AgentCollection["Guide"].Execute(new Dictionary<string, string> {  { "INPUT" , request } });