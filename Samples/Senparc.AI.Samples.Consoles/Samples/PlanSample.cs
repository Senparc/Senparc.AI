using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.KernelConfigExtensions;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.Consoles.Samples
{
    public class PlanSample
    {
        IAiHandler _aiHandler;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;
        string _userId = "Jeffrey";

        public PlanSample(IAiHandler aiHandler)
        {
            _aiHandler = aiHandler;
        }

        public async Task RunAsync()
        {

            await Console.Out.WriteLineAsync("PlanSample 开始运行。请输入需要生成的内容：");


            await Console.Out.WriteLineAsync("请输入");

            var iWantToRun = _semanticAiHandler
                           .IWantTo()
                           .ConfigModel(ConfigModel.TextCompletion, _userId, SampleHelper.Default_TextCompletion_ModeName)
                           .BuildKernel();

            //var planner = iWantToRun.ImportPlugin(new TextMemoryPlugin(iWantToRun.Kernel.Memory)).skillList;

            var dir = Path.GetDirectoryName(this.GetType().Assembly.Location);//System.IO.Directory.GetCurrentDirectory();
            //Console.WriteLine("dir:" + dir);

            var pluginsDirectory = Path.Combine(dir, "..", "..", "..", "plugins");
            //Console.WriteLine("pluginsDirectory:" + pluginsDirectory);

            await Console.Out.WriteLineAsync("Add Your Plugins, input q to finish");
            var skill = Console.ReadLine();
            while (skill != "q")
            {
                //SummarizePlugin , WriterPlugin , ...
                iWantToRun.ImportPluginFromDirectory(pluginsDirectory, skill);
                skill = Console.ReadLine();
            }

            await Console.Out.WriteLineAsync("Tell me your task:");
            //Tomorrow is Valentine's day. I need to come up with a few date ideas and e-mail them to my significant other
            var ask = Console.ReadLine();
            await Console.Out.WriteLineAsync();

#pragma warning disable SKEXP0060
            var plannerConfig = new HandlebarsPlannerOptions { MaxTokens = 2000 };
            var planner = new HandlebarsPlanner(plannerConfig);
            //var ask = "If my investment of 2130.23 dollars increased by 23%, how much would I have after I spent 5 on a latte?";


            var plan = await planner.CreatePlanAsync(iWantToRun.Kernel, ask);

            await Console.Out.WriteLineAsync("Plan:");
            await Console.Out.WriteLineAsync(plan.ToJson(true));

            // Execute the plan
            var executionSetting = iWantToRun.SemanticKernelHelper.GetExecutionSetting(new PromptConfigParameter()
            {
                Temperature = 0.01,
                TopP = 0.1,
                PresencePenalty = 0,
                FrequencyPenalty = 0,
                StopSequences = null,
            });

            var skContext = iWantToRun.CreateNewArguments();//TODO: 直返会一个对象？

            var result = await plan.InvokeAsync(skContext.context, aiRequestSettings);

            Console.WriteLine("Plan results:");
            Console.WriteLine(result.GetValue<string>());
            Console.WriteLine();

            await Console.Out.WriteLineAsync("Now system will add a new plan into your request: Rewrite the above in the style of Shakespeare. Press Enter");

            Console.ReadLine();

            //新建计划，并执行 Plan：

            string prompt = @"
{{$input}}

Rewrite the above in the style of Shakespeare.
Give me the plan less than 5 steps.
";
            var shakespeareFunction = iWantToRun.CreateFunctionFromPrompt(prompt, "shakespeare", "ShakespearePlugin", maxTokens: 2000, temperature: 0.2, topP: 0.5).function;

            var newPlan = await planner.CreatePlanAsync(ask);
            await Console.Out.WriteLineAsync("New Plan:");
            await Console.Out.WriteLineAsync(newPlan.ToJson(true));

            Console.WriteLine("Updated plan:\n");
            // Execute the plan

            var newContext = iWantToRun.CreateNewArguments();//TODO: 直返会一个对象？
            var newResult = await newPlan.InvokeAsync(newContext.context, aiRequestSettings);

            Console.WriteLine("Plan results:");
            Console.WriteLine(newResult.GetValue<string>());
            Console.WriteLine();

            await Console.Out.WriteLineAsync("== plan execute finish ==");

            await Console.Out.WriteLineAsync();

        }

    }


}
