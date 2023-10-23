using Microsoft.SemanticKernel;
using System.Collections.Generic;

namespace Senparc.AI.Kernel.Handlers
{
    public static partial class KernelConfigExtensions
    {
        /// <summary>
        /// Import a set of functions from the given skill. The functions must have the `SKFunction` attribute.
        /// Once these functions are imported, the prompt templates can use functions to import content at runtime.
        /// </summary>
        /// <param name="skillInstance">Instance of a class containing functions</param>
        /// <param name="pluginName">Name of the skill for skill collection and prompt templates. If the value is empty functions are registered in the global namespace.</param>
        /// <returns>A list of all the semantic functions found in the directory, indexed by function name.</returns>
        public static (IWantToRun iWantToRun, IDictionary<string, ISKFunction> skillList) ImportFunctions(this IWantToRun iWantToRun, object skillInstance, string pluginName = "")
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            var skillList = kernel.ImportFunctions(skillInstance, pluginName);
            return (iWantToRun, skillList);
        }

        /// <summary>
        /// A kernel extension that allows to load Semantic Functions, defined by prompt templates stored in the filesystem.
        /// A skill directory contains a set of subdirectories, one for each semantic function.
        /// This extension requires the path of the parent directory (e.g. "d:\plugins") and the name of the skill directory
        /// (e.g. "OfficePlugin"), which is used also as the "skill name" in the internal skill collection.
        ///
        /// Note: skill and function names can contain only alphanumeric chars and underscore.
        ///
        /// Example:
        /// D:\plugins\                            # parentDirectory = "D:\plugins"
        ///
        ///     |__ OfficePlugin\                  # skillDirectoryName = "SummarizeEmailThread"
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
        ///     |__ XboxPlugin\                    # another skill, etc.
        ///
        ///         |__ MessageFriend
        ///             |__ skprompt.txt
        ///             |__ config.json
        ///         |__ LaunchGame
        ///             |__ skprompt.txt
        ///             |__ config.json
        ///
        /// See https://github.com/microsoft/semantic-kernel/tree/main/samples/plugins for some plugins in our repo.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="parentDirectory">Directory containing the skill directory, e.g. "d:\myAppPlugins"</param>
        /// <param name="skillDirectoryName">Name of the directory containing the selected skill, e.g. "StrategyPlugin"</param>
        /// <returns>A list of all the semantic functions found in the directory, indexed by function name.</returns>
        /// <returns></returns>
        public static (IWantToRun iWantToRun, IDictionary<string, ISKFunction> skillList) ImportPluginFromDirectory(this IWantToRun iWantToRun, string parentDirectory, string skillDirectoryName)
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            var kernel = helper.GetKernel();

            var skillList = kernel.ImportSemanticFunctionsFromDirectory(parentDirectory, skillDirectoryName);
            return (iWantToRun, skillList);
        }
    }
}
