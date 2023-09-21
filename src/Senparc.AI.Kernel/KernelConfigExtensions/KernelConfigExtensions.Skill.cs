using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
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

        /// <summary>
        /// A kernel extension that allows to load Semantic Functions, defined by prompt templates stored in the filesystem.
        /// A skill directory contains a set of subdirectories, one for each semantic function.
        /// This extension requires the path of the parent directory (e.g. "d:\skills") and the name of the skill directory
        /// (e.g. "OfficeSkill"), which is used also as the "skill name" in the internal skill collection.
        ///
        /// Note: skill and function names can contain only alphanumeric chars and underscore.
        ///
        /// Example:
        /// D:\skills\                            # parentDirectory = "D:\skills"
        ///
        ///     |__ OfficeSkill\                  # skillDirectoryName = "SummarizeEmailThread"
        ///
        ///         |__ ScheduleMeeting           # semantic function
        ///             |__ skprompt.txt          # prompt template
        ///             |__ config.json           # settings (optional file)
        ///
        ///         |__ SummarizeEmailThread      # semantic function
        ///             |__ skprompt.txt          # prompt template
        ///             |__ config.json           # settings (optional file)
        ///
        ///         |__ MergeWordAndExcelDocs     # semantic function
        ///             |__ skprompt.txt          # prompt template
        ///             |__ config.json           # settings (optional file)
        ///
        ///     |__ XboxSkill\                    # another skill, etc.
        ///
        ///         |__ MessageFriend
        ///             |__ skprompt.txt
        ///             |__ config.json
        ///         |__ LaunchGame
        ///             |__ skprompt.txt
        ///             |__ config.json
        ///
        /// See https://github.com/microsoft/semantic-kernel/tree/main/samples/skills for some skills in our repo.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="parentDirectory">Directory containing the skill directory, e.g. "d:\myAppSkills"</param>
        /// <param name="skillDirectoryName">Name of the directory containing the selected skill, e.g. "StrategySkill"</param>
        /// <returns>A list of all the semantic functions found in the directory, indexed by function name.</returns>
        /// <returns></returns>
        public static (IWantToRun iWantToRun, IDictionary<string, ISKFunction> skillList) ImportSkillFromDirectory(this IWantToRun iWantToRun, string parentDirectory, string skillDirectoryName)
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            var kernel = helper.GetKernel();
                                   
            var skillList = kernel.ImportSemanticSkillFromDirectory(parentDirectory, skillDirectoryName);
            return (iWantToRun, skillList);
        }
    }
}
