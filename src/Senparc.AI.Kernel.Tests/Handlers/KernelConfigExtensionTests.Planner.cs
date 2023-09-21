using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Skills.Core;
using Senparc.AI.Interfaces;
using Senparc.AI.Tests;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Kernel.Handlers.Tests
{
    public partial class KernelConfigExtensionTests
    {
        [TestMethod]
        public async Task PlannerTest()
        {
            //TODO: 测试每个阶段时间，Planner 比较耗时

            //准备
            var serviceProvider = BaseTest.serviceProvider;
            var handler = serviceProvider.GetRequiredService<IAiHandler>()
                            as SemanticAiHandler;
            var userId = "JeffreySu";

            var iWantToRun = handler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextCompletion, userId, "text-davinci-003")
                 .BuildKernel();

            //var planner = iWantToRun.ImportSkill(new PlannerSkill(iWantToRun.Kernel)).skillList;
            //var plannerOld = iWantToRun.ImportSkill(new TextMemorySkill(iWantToRun.Kernel.Memory)).skillList;

            var dir = System.IO.Directory.GetCurrentDirectory();
            var skillsDirectory = Path.Combine(dir, "..", "..", "..", "skills");
            Console.WriteLine("skillsDirectory:" + skillsDirectory);
            iWantToRun.ImportSkillFromDirectory(skillsDirectory, "SummarizeSkill");
            iWantToRun.ImportSkillFromDirectory(skillsDirectory, "WriterSkill");



            var planner = new SequentialPlanner(iWantToRun.Kernel);
            //var ask = "If my investment of 2130.23 dollars increased by 23%, how much would I have after I spent 5 on a latte?";
            var ask = "Tomorrow is Valentine's day. I need to come up with a few date ideas and e-mail them to my significant other.";
            var plan = await planner.CreatePlanAsync(ask);

            var requestSettings = new Microsoft.SemanticKernel.AI.TextCompletion.CompleteRequestSettings()
            {
                Temperature = 0.01,
                MaxTokens = 5000,
                TopP = 0.1,
            };

            // Execute the plan
            var result = await plan.InvokeAsync(settings: requestSettings);

            Console.WriteLine("Plan results:");
            Console.WriteLine(result.Result);

            Console.WriteLine();

            Console.WriteLine("New plan results:");
            var shakespeareFunction = iWantToRun.CreateSemanticFunction(ask, "shakespeare", "ShakespeareSkill", maxTokens: 5000, temperature: 0.2, topP: 0.5).function;

            // Execute the plan
            plan = await planner.CreatePlanAsync(ask);
            result = await plan.InvokeAsync(settings: requestSettings);

            Console.WriteLine("Plan results:");
            Console.WriteLine(result.Result);


            //            var dir = System.IO.Directory.GetCurrentDirectory();
            //            Console.WriteLine("dir:" + dir);

            //            var skillsDirectory = Path.Combine(dir, "..", "..", "..", "skills");
            //            Console.WriteLine("skillsDirectory:" + skillsDirectory);
            //            iWantToRun.ImportSkillFromDirectory(skillsDirectory, "SummarizeSkill");
            //            iWantToRun.ImportSkillFromDirectory(skillsDirectory, "WriterSkill");

            //            //创建 Plan：
            //            var ask = "Tomorrow is Valentine's day. I need to come up with a few date ideas and e-mail them to my significant other.";
            //            var request = iWantToRun.CreateRequest(ask, planner["CreatePlan"]);
            //            var originalPlan = await iWantToRun.RunAsync(request);

            //            var plannResult = originalPlan.Result.Variables.ToPlan().PlanString;
            //            Assert.IsTrue(!plannResult.IsNullOrEmpty());

            //            await Console.Out.WriteLineAsync("Original plan:");
            //            await Console.Out.WriteLineAsync(plannResult);


            //            //新建计划，并执行 Plan：

            //            string prompt = @"
            //{{$input}}

            //Rewrite the above in the style of Shakespeare.
            //";
            //            var shakespeareFunction = iWantToRun.CreateSemanticFunction(prompt, "shakespeare", "ShakespeareSkill", maxTokens: 2000, temperature: 0.2, topP: 0.5).function;

            //            var newRequest = iWantToRun.CreateRequest(ask, planner["CreatePlan"], shakespeareFunction);
            //            var newPlan = await iWantToRun.RunAsync(newRequest);
            //            var newPlanResult = newPlan.Result.Variables.ToPlan().PlanString;
            //            Assert.IsTrue(!newPlanResult.IsNullOrEmpty());

            //            Console.WriteLine("Updated plan:\n");
            //            Console.WriteLine(newPlanResult);


            //            //Excute
            //            var executionResults = newPlan.Result;

            //            int step = 1;
            //            int maxSteps = 10;
            //            while (!executionResults.Variables.ToPlan().IsComplete && step < maxSteps)
            //            {
            //                var stepRequest = iWantToRun.CreateRequest(executionResults.Variables, false, planner["ExecutePlan"]);
            //                var results = (await iWantToRun.RunAsync(stepRequest)).Result;
            //                if (results.Variables.ToPlan().IsSuccessful)
            //                {
            //                    Console.WriteLine($"Step {step} - Execution results:\n");
            //                    Console.WriteLine(results.Variables.ToPlan().PlanString);

            //                    if (results.Variables.ToPlan().IsComplete)
            //                    {
            //                        Console.WriteLine($"Step {step} - COMPLETE!");
            //                        Console.WriteLine(results.Variables.ToPlan().Result);
            //                        break;
            //                    }
            //                }
            //                else
            //                {
            //                    Console.WriteLine($"Step {step} - Execution failed:");
            //                    Console.WriteLine("Error Message:" + results.LastException?.Message);
            //                    Console.WriteLine(results.Variables.ToPlan().Result);
            //                    break;
            //                }

            //                executionResults = results;
            //                step++;
            //                Console.WriteLine("");
            //            }

            //            await Console.Out.WriteLineAsync("== plan execute finish ==");
        }
    }
}
