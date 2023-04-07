using Microsoft.SemanticKernel.Orchestration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Kernel.Handlers
{
    public static partial class KernelConfigExtensions
    {
        /// <summary>
        /// Import a set of functions from the given skill. The functions must have the `SKFunction` attribute.
        /// Once these functions are imported, the prompt templates can use functions to import content at runtime.
        /// </summary>
        /// <param name="skillInstance">Instance of a class containing functions</param>
        /// <param name="skillName">Name of the skill for skill collection and prompt templates. If the value is empty functions are registered in the global namespace.</param>
        /// <returns>A list of all the semantic functions found in the directory, indexed by function name.</returns>
        public static (IWantToRun iWantToRun, IDictionary<string, ISKFunction> skillList) ImportSkill(this IWantToRun iWantToRun, object skillInstance, string skillName = "")
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            var skillList = kernel.ImportSkill(skillInstance, skillName);
            return (iWantToRun, skillList);
        }
    }
}
