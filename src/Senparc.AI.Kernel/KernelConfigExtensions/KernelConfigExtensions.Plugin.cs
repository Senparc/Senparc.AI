using Microsoft.SemanticKernel;
using System;
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
        [Obsolete($"This method is obsolete, please use {nameof(ImportPluginFromObject)} instead.", true)]
        public static (IWantToRun iWantToRun, KernelPlugin kernelPlugin) ImportFunctions(this IWantToRun iWantToRun, object skillInstance, string pluginName = null)
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            var kernelPlugin = kernel.ImportPluginFromObject(skillInstance, pluginName);
            return (iWantToRun, kernelPlugin);
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
        [Obsolete($"This method is obsolete, please use {nameof(ImportPluginFromPromptDirectory)} instead.", true)]
        public static (IWantToRun iWantToRun, KernelPlugin kernelPlugin) ImportPluginFromDirectory(this IWantToRun iWantToRun, string parentDirectory, string skillDirectoryName)
        {
            return ImportPluginFromDirectory(iWantToRun, parentDirectory, skillDirectoryName);
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
        /// <param name="pluginName">Name of the directory containing the selected skill, e.g. "StrategyPlugin"</param>
        /// <param name="promptTemplateFactory">The Microsoft.SemanticKernel.IPromptTemplateFactory to use when interpreting discovered prompts into Microsoft.SemanticKernel.IPromptTemplates. If null, a default factory will be used.
        /// <param name="throwExceptionWhenSamePluginNameExisted">If true, throw exception when the same plugin name existed.</param>
        /// <returns>A list of all the semantic functions found in the directory, indexed by function name.</returns>
        /// <returns></returns>
        public static (IWantToRun iWantToRun, KernelPlugin kernelPlugin) ImportPluginFromPromptDirectory(this IWantToRun iWantToRun, string parentDirectory, string pluginName, IPromptTemplateFactory? promptTemplateFactory = null, bool throwExceptionWhenSamePluginNameExisted = false)
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            var kernel = helper.GetKernel();

            KernelPlugin kernelPlugin;
            if (!throwExceptionWhenSamePluginNameExisted && kernel.Plugins.Contains(pluginName))
            {
                kernelPlugin = kernel.Plugins[pluginName];
            }
            else
            {
                kernelPlugin = kernel.ImportPluginFromPromptDirectory(parentDirectory, pluginName, promptTemplateFactory);
            }

            return (iWantToRun, kernelPlugin);
        }

        /// <summary>
        /// Creates a plugin that wraps the specified target object and imports it into the kernel's plugin collection.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="target">The instance of the class to be wrapped.</param>
        /// <param name="throwExceptionWhenSamePluginNameExisted">If true, throw exception when the same plugin name existed.</param>
        /// <param name="pluginName">Name of the plugin for function collection and prompt templates. If the value is null, a plugin name is derived from the type of the target.</param>
        /// <returns>A Microsoft.SemanticKernel.KernelPlugin containing Microsoft.SemanticKernel.KernelFunctions for all relevant members of target.</returns>
        /// <remarks>Public methods that have the Microsoft.SemanticKernel.KernelFunctionFromPrompt attribute will be included in the plugin.</remarks>
        public static (IWantToRun iWantToRun, KernelPlugin kernelPlugin) ImportPluginFromObject(this IWantToRun iWantToRun, object target, string? pluginName = null, bool throwExceptionWhenSamePluginNameExisted = false)
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            var kernel = helper.GetKernel();

            KernelPlugin kernelPlugin;
            pluginName ??= target.GetType().Name;

            if (!throwExceptionWhenSamePluginNameExisted && kernel.Plugins.Contains(pluginName))
            {
                kernelPlugin = kernel.Plugins[pluginName];
            }
            else
            {
                kernelPlugin = kernel.ImportPluginFromObject(target, pluginName);
            }

            return (iWantToRun, kernelPlugin);
        }

        /// <summary>
        /// The Microsoft.SemanticKernel.Kernel containing services, plugins, and other state for use throughout the operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iWantToRun"></param>
        /// <param name="pluginName">Name of the plugin for function collection and prompt templates. If the value is null, a plugin name is derived from the type of the T.</param>
        /// <param name="throwExceptionWhenSamePluginNameExisted">If true, throw exception when the same plugin name existed.</param>
        /// <returns>A Microsoft.SemanticKernel.KernelPlugin containing Microsoft.SemanticKernel.KernelFunctions for all relevant members of T.</returns>
        public static (IWantToRun iWantToRun, KernelPlugin kernelPlugin) ImportPluginFromType<T>(this IWantToRun iWantToRun, string? pluginName = null, bool throwExceptionWhenSamePluginNameExisted = false)
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            var kernel = helper.GetKernel();

            KernelPlugin kernelPlugin;
            pluginName ??= typeof(T).Name;

            if (!throwExceptionWhenSamePluginNameExisted && kernel.Plugins.Contains(pluginName))
            {
                kernelPlugin = kernel.Plugins[pluginName];
            }
            else
            {
                kernelPlugin = kernel.ImportPluginFromType<T>(pluginName);
            }

            return (iWantToRun, kernelPlugin);
        }

        /// <summary>
        ///  Creates a plugin that contains the specified functions and imports it into the kernel's plugin collection.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="pluginName">The name for the plugin.</param>
        /// <param name="functions">The initial functions to be available as part of the plugin.</param>
        /// <param name="throwExceptionWhenSamePluginNameExisted">If true, throw exception when the same plugin name existed.</param>
        /// <returns>A Microsoft.SemanticKernel.KernelPlugin containing the functions provided in functions.</returns>
        public static (IWantToRun iWantToRun, KernelPlugin kernelPlugin) ImportPluginFromFunctions(this IWantToRun iWantToRun, string pluginName, IEnumerable<KernelFunction>? functions = null, bool throwExceptionWhenSamePluginNameExisted = false)
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            var kernel = helper.GetKernel();

            KernelPlugin kernelPlugin;

            if (!throwExceptionWhenSamePluginNameExisted && kernel.Plugins.Contains(pluginName))
            {
                kernelPlugin = kernel.Plugins[pluginName];
            }
            else
            {
                kernelPlugin = kernel.ImportPluginFromFunctions(pluginName, functions);
            }

            return (iWantToRun, kernelPlugin);
        }


    }
}
