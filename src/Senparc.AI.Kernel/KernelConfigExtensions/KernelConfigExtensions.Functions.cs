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
        public static (IWantToRun iWantToRun, KernelPlugin skillList) ImportFunctions(this IWantToRun iWantToRun, object skillInstance, string pluginName = "")
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            var skillList = kernel.ImportPluginFromObject(skillInstance, pluginName);
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
        //     A plugin directory contains a set of subdirectories, one for each function in
        //     the form of a prompt. This method accepts the path of the plugin directory. Each
        //     subdirectory's name is used as the function name and may contain only alphanumeric
        //     chars and underscores.
        //
        //     The following directory structure, with pluginDirectory = "D:\plugins\OfficePlugin",
        //
        //     will create a plugin with three functions:
        //     D:\plugins\
        //     |__ OfficePlugin\ # pluginDirectory
        //     |__ ScheduleMeeting # function directory
        //     |__ skprompt.txt # prompt template
        //     |__ config.json # settings (optional file)
        //     |__ SummarizeEmailThread # function directory
        //     |__ skprompt.txt # prompt template
        //     |__ config.json # settings (optional file)
        //     |__ MergeWordAndExcelDocs # function directory
        //     |__ skprompt.txt # prompt template
        //     |__ config.json # settings (optional file)
        //
        //     See https://github.com/microsoft/semantic-kernel/tree/main/samples/plugins for
        //     examples in the Semantic Kernel repository.  
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="parentDirectory">Directory containing the skill directory, e.g. "d:\myAppPlugins"</param>
        /// <param name="skillDirectoryName">Name of the directory containing the selected skill, e.g. "StrategyPlugin"</param>
        /// <returns>A list of all the semantic functions found in the directory, indexed by function name.</returns>
        /// <returns></returns>
        public static (IWantToRun iWantToRun, KernelPlugin skillList) ImportPluginFromDirectory(this IWantToRun iWantToRun, string parentDirectory, string skillDirectoryName)
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            var kernel = helper.GetKernel();

            var skillList = kernel.ImportPluginFromPromptDirectory(parentDirectory, skillDirectoryName);

            return (iWantToRun, skillList);
        }
    }
}
