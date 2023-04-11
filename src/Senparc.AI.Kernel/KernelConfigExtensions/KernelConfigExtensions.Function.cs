using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;
using Senparc.AI.Entities;
using Senparc.AI.Kernel.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Kernel.Handlers
{
            
    /* 注意：所有 Functiuon 添后加都必须执行 iWantToRun.Functions.Add(function); */

    public static partial class KernelConfigExtensions
    {
        /// <summary>
        /// Build and register a function in the internal skill collection.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="skillName">Name of the skill containing the function. The name can contain only alphanumeric chars + underscore.</param>
        /// <param name="functionName">Name of the semantic function. The name can contain only alphanumeric chars + underscore.</param>
        /// <param name="functionConfig">Function configuration, e.g. I/O params, AI settings, localization details, etc.</param>
        /// <param name="promptConfigPara"></param>
        /// <param name="skPrompt"></param>
        /// <returns>A C# function wrapping AI logic, usually defined with natural language</returns>
        public static (IWantToRun iWantToRun, ISKFunction newFunction) RegisterSemanticFunction(this IWantToRun iWantToRun, string skillName, string functionName, PromptConfigParameter promptConfigPara, string? skPrompt = Senparc.AI.DefaultSetting.DEFAULT_PROMPT_FOR_CHAT)
        {
            var promptConfig = new PromptTemplateConfig
            {
                Completion =
                    {
                        MaxTokens = promptConfigPara.MaxTokens.Value,
                        Temperature = promptConfigPara.Temperature.Value,
                        TopP = promptConfigPara.TopP.Value,
                    }
            };
            //TODO:自动匹配

            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;
            var helper = iWantToRun.SemanticKernelHelper;
            var kernel = iWantToRun.Kernel;

            var promptTemplate = new PromptTemplate(skPrompt, promptConfig, kernel);
            var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);

            var newFunction = kernel.RegisterSemanticFunction(skillName/*"ChatBot"*/, functionName /*"Chat"*/, functionConfig);

            var aiContext = new SenparcAiContext();

            //TODO:独立 Context
            var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);
            var history = "";
            aiContext.SubContext.Set(serviceId, history);

            //iWantToRun.ISKFunction = chatFunction;
            iWantToRun.StoredAiContext = aiContext;
            iWantToRun.PromptConfigParameter = promptConfigPara;
            iWantToRun.Functions.Add(newFunction);

            return (iWantToRun, newFunction);
        }

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
            var kernel = iWantToRun.Kernel;
            var function = kernel.CreateSemanticFunction(promptTemplate, functionName, skillName, description, maxTokens, temperature, topP, presencePenalty, frequencyPenalty, stopSequences);
            iWantToRun.Functions.Add(function);
            return (iWantToRun, function);
        }

        /// <summary>
        /// Build and register a function in the internal skill collection, in a global generic skill.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="skillName">Name of the skill containing the function. The name can contain only alphanumeric chars + underscore.</param>
        /// <param name="functionName">Name of the semantic function. The name can contain only alphanumeric chars + underscore.</param>
        /// <param name="functionConfig">Function configuration, e.g. I/O params, AI settings, localization details, etc.</param>
        /// <returns>A C# function wrapping AI logic, usually defined with natural language</returns>
        public static (IWantToRun iWantToRun, ISKFunction function) RegisterSemanticFunction(this IWantToRun iWantToRun, 
                string skillName,
                string functionName,
                SemanticFunctionConfig functionConfig)
        {
            var kernel = iWantToRun.Kernel;
            var function = kernel.RegisterSemanticFunction(skillName, functionName, functionConfig);
            iWantToRun.Functions.Add(function);
            return (iWantToRun, function);
        }

        /// <summary>
        /// Build and register a function in the internal skill collection, in a global generic skill.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="functionName">Name of the semantic function. The name can contain only alphanumeric chars + underscore.</param>
        /// <param name="functionConfig">Function configuration, e.g. I/O params, AI settings, localization details, etc.</param>
        /// <returns>A C# function wrapping AI logic, usually defined with natural language</returns>
        public static (IWantToRun iWantToRun, ISKFunction function) RegisterSemanticFunction(this IWantToRun iWantToRun,
                string functionName,
                SemanticFunctionConfig functionConfig)
        {
            var kernel = iWantToRun.Kernel;
            var function = kernel.RegisterSemanticFunction(functionName, functionConfig);
            iWantToRun.Functions.Add(function);
            return (iWantToRun, function);
        }

    }
}
