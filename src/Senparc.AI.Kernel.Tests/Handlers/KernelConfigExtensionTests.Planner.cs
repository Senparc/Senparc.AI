using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Interfaces;
using Senparc.AI.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.AI.Kernel.Helpers;
using Senparc.AI.Kernel.KernelConfigExtensions;
using Microsoft.SemanticKernel.CoreSkills;
using Microsoft.SemanticKernel;

namespace Senparc.AI.Kernel.Handlers.Tests
{
    public partial class KernelConfigExtensionTests
    {
        [TestMethod]
        public async Task PlannerTest()
        {
            var serviceProvider = BaseTest.serviceProvider;

            var handler = serviceProvider.GetRequiredService<IAiHandler>()
                            as SemanticAiHandler;
            var userId = "JeffreySu";

            //测试 TextEmbedding
            var iWantToRun = handler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextCompletion, userId, "text-davinci-003")
                 .BuildKernel();

            var planner = iWantToRun.ImportSkill(new PlannerSkill(iWantToRun.Kernel)).skillList;


            var dir = System.IO.Directory.GetCurrentDirectory();
            Console.WriteLine("dir:" + dir);

            var skillsDirectory = Path.Combine(dir, "..", "..", "..", "skills");
            iWantToRun.ImportSkillFromDirectory(skillsDirectory, "SummarizeSkill");
            iWantToRun.ImportSkillFromDirectory(skillsDirectory, "WriterSkill");

            var ask = "Tomorrow is Valentine's day. I need to come up with a few date ideas and e-mail them to my significant other.";

            var request = iWantToRun.GetRequest(ask, planner["CreatePlan"]);

            var originalPlan = await iWantToRun.RunAsync(request);


        }
    }
}
