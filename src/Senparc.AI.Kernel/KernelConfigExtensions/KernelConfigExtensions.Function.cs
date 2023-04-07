using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Kernel.Handlers
{
    public static partial class KernelConfigExtensions
    {
        /// <summary>
        /// Define a string-to-string semantic function, with no direct support for input context.
        /// The function can be referenced in templates and will receive the context, but when invoked programmatically you
        /// can only pass in a string in input and receive a string in output.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="promptTemplate">Plain language definition of the semantic function, using SK template language</param>
        /// <param name="functionName">A name for the given function. The name can be referenced in templates and used by the pipeline planner.</param>
        /// <param name="skillName">Optional skill name, for namespacing and avoid collisions</param>
        /// <param name="description">Optional description, useful for the planner</param>
        /// <param name="maxTokens">Max number of tokens to generate</param>
        /// <param name="temperature">Temperature parameter passed to LLM</param>
        /// <param name="topP">Top P parameter passed to LLM</param>
        /// <param name="presencePenalty">Presence Penalty parameter passed to LLM</param>
        /// <param name="frequencyPenalty">Frequency Penalty parameter passed to LLM</param>
        /// <param name="stopSequences">Strings the LLM will detect to stop generating (before reaching max tokens)</param>
        /// <returns>A function ready to use</returns>
        public static (IWantToRun iWantToRun, ISKFunction function) CreateSemanticFunction(this IWantToRun iWantToRun,
                string promptTemplate,
                string? functionName = null,
        string skillName = "",
                string? description = null,
                int maxTokens = 256,
                double temperature = 0,
                double topP = 0,
                double presencePenalty = 0,
                double frequencyPenalty = 0,
                IEnumerable<string>? stopSequences = null)
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            var function = kernel.CreateSemanticFunction(promptTemplate, functionName, skillName, description, maxTokens, temperature, topP, presencePenalty, frequencyPenalty, stopSequences);
            return (iWantToRun, function);
        }
    }
}
