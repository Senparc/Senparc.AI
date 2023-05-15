using Microsoft.SemanticKernel.CoreSkills;
using Microsoft.SemanticKernel.Orchestration;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;

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
                           .ConfigModel(ConfigModel.TextCompletion, _userId, "text-davinci-003")
                           .BuildKernel();

            var planner = iWantToRun.ImportSkill(new PlannerSkill(iWantToRun.Kernel)).skillList;

            var dir = Path.GetDirectoryName(this.GetType().Assembly.Location);//System.IO.Directory.GetCurrentDirectory();
            //Console.WriteLine("dir:" + dir);

            var skillsDirectory = Path.Combine(dir, "..", "..", "..", "skills");
            //Console.WriteLine("skillsDirectory:" + skillsDirectory);

            await Console.Out.WriteLineAsync("Add Your Skills, input q to finish");
            var skill = Console.ReadLine();
            while (skill != "q")
            {
                //SummarizeSkill , WriterSkill , ...
                iWantToRun.ImportSkillFromDirectory(skillsDirectory, skill);
                skill = Console.ReadLine();
            }

            await Console.Out.WriteLineAsync("Tell me your task:");
            //Tomorrow is Valentine's day. I need to come up with a few date ideas and e-mail them to my significant other
            var ask = Console.ReadLine();
            await Console.Out.WriteLineAsync();

            var request = iWantToRun.CreateRequest(ask, planner["CreatePlan"]);
            var originalPlan = await iWantToRun.RunAsync(request);

            var plannResult = originalPlan.Result.Variables.ToPlan().PlanString;
            await Console.Out.WriteLineAsync("Plan Created!");
            await Console.Out.WriteLineAsync(plannResult);

            await Console.Out.WriteLineAsync("Now system will add a new plan into your request: Rewrite the above in the style of Shakespeare. Press Enter");

            Console.ReadLine();

            //新建计划，并执行 Plan：

            string prompt = @"
{{$input}}

Rewrite the above in the style of Shakespeare.
Give me the plan less than 5 steps.
";
            var shakespeareFunction = iWantToRun.CreateSemanticFunction(prompt, "shakespeare", "ShakespeareSkill", maxTokens: 2000, temperature: 0.2, topP: 0.5).function;

            var newRequest = iWantToRun.CreateRequest(ask, planner["CreatePlan"], shakespeareFunction);
            var newPlan = await iWantToRun.RunAsync(newRequest);
            var newPlanResult = newPlan.Result.Variables.ToPlan().PlanString;

            Console.WriteLine("Updated plan:\n");
            Console.WriteLine(newPlanResult);

            await Console.Out.WriteLineAsync("Press Enter to Now executing the plan...");
            Console.ReadLine();

            var executionResults = newPlan.Result;

            int step = 1;
            int maxSteps = 10;
            while (!executionResults.Variables.ToPlan().IsComplete && step < maxSteps)
            {
                var stepRequest = iWantToRun.CreateRequest(executionResults.Variables, false, planner["ExecutePlan"]);
                var results = (await iWantToRun.RunAsync(stepRequest)).Result;
                if (results.Variables.ToPlan().IsSuccessful)
                {
                    Console.WriteLine($"Step {step} - Execution results:\n");
                    Console.WriteLine(results.Variables.ToPlan().PlanString);

                    if (results.Variables.ToPlan().IsComplete)
                    {
                        Console.WriteLine($"Step {step} - COMPLETE!");
                        Console.WriteLine(results.Variables.ToPlan().Result);
                        break;
                    }
                }
                else
                {
                    Console.WriteLine($"Step {step} - Execution failed:");
                    Console.WriteLine("Error Message:" + results.LastException?.Message);
                    Console.WriteLine(results.Variables.ToPlan().Result);
                    break;
                }

                executionResults = results;
                step++;
                Console.WriteLine("");
            }

            await Console.Out.WriteLineAsync("== plan execute finish ==");

            await Console.Out.WriteLineAsync();

        }

    }


}
