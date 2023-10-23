using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;
using Microsoft.VisualStudio.TestPlatform;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.KernelConfigExtensions;
using Senparc.AI.Kernel.Tests.BaseSupport;
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
                 .ConfigModel(ConfigModel.TextCompletion, userId, KernelTestBase.Default_TextCompletion)
                 .BuildKernel();

            //var planner = iWantToRun.ImportSkill(new PlannerSkill(iWantToRun.Kernel)).skillList;
            //var plannerOld = iWantToRun.ImportSkill(new TextMemorySkill(iWantToRun.Kernel.Memory)).skillList;

            var dir = System.IO.Directory.GetCurrentDirectory();
            var pluginsDirectory = Path.Combine(dir, "..", "..", "..", "plugins");
            Console.WriteLine("pluginsDirectory:" + pluginsDirectory);
            iWantToRun.ImportSkillFromDirectory(pluginsDirectory, "SummarizeSkill");
            iWantToRun.ImportSkillFromDirectory(pluginsDirectory, "WriterSkill");

            //var ask = "If my investment of 2130.23 dollars increased by 23%, how much would I have after I spent 5 on a latte?";
            var ask = "Tomorrow is Valentine's day. I need to come up with a few date ideas and e-mail them to my significant other. Limit the output words to 300.";

            AIRequestSettings aiRequestSettings = new AIRequestSettings()
            {
                ExtensionData = new Dictionary<string, object>()
                        {
                            { "Temperature",0.1 },
                            { "TopP", 0.5 },
                            { "MaxTokens", 1000 }
                        }
            };

            {

                var planner = new SequentialPlanner(iWantToRun.Kernel);
                var plan = await planner.CreatePlanAsync(ask);

                // Execute the plan
                var skContext = iWantToRun.CreateNewContext().context;
                var result = await plan.InvokeAsync(skContext, aiRequestSettings);

                Console.WriteLine("Plan results:");
                Console.WriteLine(result.GetValue<string>());

                Console.WriteLine();
            }

            {
                // Execute the plan

                Console.WriteLine("New plan results:");
                var shakespeareFunction = iWantToRun.CreateSemanticFunction(ask, "shakespeare", "ShakespeareSkill", maxTokens: 2000, temperature: 0.2, topP: 0.5).function;

                var newPlanner = new SequentialPlanner(iWantToRun.Kernel, new SequentialPlannerConfig()
                {
                    MaxTokens = 1000,
                });

                var newPlan = await newPlanner.CreatePlanAsync(ask);
                //var newPlan = await iWantToRun.RunAsync<string>(newPlan);

                await Console.Out.WriteLineAsync("NewPlans:");
                foreach (var item in newPlan.Steps)
                {
                    await Console.Out.WriteLineAsync(item.ToJson());
                }

                var newResult = await newPlan.InvokeAsync(iWantToRun.Kernel, requestSettings: aiRequestSettings); // await iWantToRun.RunAsync<string>(newPlan);

                Console.WriteLine("Plan results:");
                Console.WriteLine(newResult);
            }
        }
    }
}
