
#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Jeffrey Su & Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/JeffreySu/WeiXinMPSDK/blob/master/license.md

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2023 Senparc
  
    文件名：KernelConfigExtensions.Function.cs
    文件功能描述：
    
    
    创建标识：Senparc - 20150211
    
    修改标识：Felixj - 20231207
    修改描述：修复运行Samples时导致JSONException的问题

----------------------------------------------------------------*/


using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TemplateEngine;
using Senparc.AI.Entities;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Helpers;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Senparc.AI.Kernel.Handlers
{
    /* 注意：所有 Functiuon 添后加都必须执行 iWantToRun.Functions.Add(function); */

    public static partial class KernelConfigExtensions
    {
        #region SemanticFunction

        /// <summary>
        /// Build and register a function in the internal skill collection.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="promptTemplate">Prompt template for the function.</param>
        /// <param name="executionSettings">Default execution settings to use when invoking this prompt function.</param>
        /// <param name="functionName">The name to use for the function. If null, it will default to a randomly generated name.</param>
        /// <param name="description">The description to use for the function.</param>
        /// <param name="templateFormat">The template format of <paramref name="promptTemplate"/>. This must be provided if <paramref name="promptTemplateFactory"/> is not null.</param>
        /// <param name="promptTemplateFactory">
        /// The <see cref="IPromptTemplateFactory"/> to use when interpreting the <paramref name="promptTemplate"/> into a <see cref="IPromptTemplate"/>.
        /// If null, a default factory will be used.
        /// </param>
        /// <returns>(IWantToRun iWantToRun, KernelFunction newFunction)</returns>
        public static (IWantToRun iWantToRun, KernelFunction newFunction) CreateFunctionFromPrompt(this IWantToRun iWantToRun, string promptTemplate,
            PromptConfigParameter? promptConfigPara = null,
            string? functionName = null,
            string? description = null,
            string? templateFormat = null,
            IPromptTemplateFactory? promptTemplateFactory = null
            /*string? skPrompt = Senparc.AI.DefaultSetting.DEFAULT_PROMPT_FOR_CHAT*/)
        {
            promptConfigPara ??= new PromptConfigParameter();

            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;
            var helper = iWantToRun.SemanticKernelHelper;
            var kernel = iWantToRun.Kernel;

            var executionSetting = helper.GetExecutionSetting(promptConfigPara);

            //var promptTemplateConfig = new PromptTemplateConfig();
            //promptTemplateConfig.ModelSettings.Add(aiRequestSettings);

            //var promptTemplateFactory = new BasicPromptTemplateFactory();
            //var promptTemplate = promptTemplateFactory.Create(
            //    skPrompt, // Prompt template defined in natural language
            //    promptTemplateConfig // Prompt configuration
            //);

            //var promptTemplateFactory = new KernelPromptTemplateFactory();


            var newFunction =
                kernel.CreateFunctionFromPrompt(promptTemplate, executionSetting, functionName, description, templateFormat, promptTemplateFactory);

            var aiContext = new SenparcAiArguments();

            //TODO:独立 Context
            var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);
            var history = "";
            aiContext.KernelArguments.Set(serviceId, history);

            //iWantToRun.KernelFunction = chatFunction;
            iWantToRun.StoredAiArguments = aiContext;
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
        /// <param name="pluginName">Optional skill name, for namespacing and avoid collisions</param>
        /// <param name="description">Optional description, useful for the planner</param>
        /// <param name="maxTokens">Max number of tokens to generate</param>
        /// <param name="temperature">Temperature parameter passed to LLM</param>
        /// <param name="topP">Top P parameter passed to LLM</param>
        /// <param name="presencePenalty">Presence Penalty parameter passed to LLM</param>
        /// <param name="frequencyPenalty">Frequency Penalty parameter passed to LLM</param>
        /// <param name="stopSequences">Strings the LLM will detect to stop generating (before reaching max tokens)</param>
        /// <returns>A function ready to use</returns>
        public static (IWantToRun iWantToRun, KernelFunction function) CreateFunctionFromPrompt(this IWantToRun iWantToRun,
            string promptTemplate,
            string? functionName = null,
            string? description = null,
            int maxTokens = 256,
            double temperature = 0,
            double topP = 0,
            double presencePenalty = 0,
            double frequencyPenalty = 0,
            IList<string>? stopSequences = null,
            string? templateFormat = null,
            IPromptTemplateFactory? promptTemplateFactory = null)
        {

            PromptExecutionSettings executionSettings = iWantToRun
                .SemanticKernelHelper.GetExecutionSetting(
                     maxTokens: maxTokens,
                     temperature: temperature,
                     topP: topP,
                     presencePenalty: presencePenalty,
                     frequencyPenalty: frequencyPenalty,
                     stopSequences: stopSequences
                    );

            //var promptTemplateFactory = new KernelPromptTemplateFactory();
            //var promptTemplateConfig = new PromptTemplateConfig()
            //{
            //    ExecutionSettings = new List<PromptExecutionSettings> { executionSettings }
            //};
            //var promptTemplate = promptTemplateFactory.Create(promptTemplateConfig);

            var kernel = iWantToRun.Kernel;
            var function = kernel.CreateFunctionFromPrompt(promptTemplate, executionSettings, functionName, description, templateFormat, promptTemplateFactory);
            iWantToRun.Functions.Add(function);
            return (iWantToRun, function);
        }

        ///// <summary>
        ///// Build and register a function in the internal skill collection, in a global generic skill.
        ///// </summary>
        ///// <param name="iWantToRun"></param>
        //public static (IWantToRun iWantToRun, KernelFunction function) CreateFunctionFromPrompt(this IWantToRun iWantToRun,
        //    string promptTemplate,
        //    string functionName,
        //    PromptConfigParameter promptConfigPara,
        //    string? description = null)
        //{
        //    var kernel = iWantToRun.Kernel;
        //    var helper = iWantToRun.SemanticKernelHelper;
        //    var executionSetting = helper.GetExecutionSetting(promptConfigPara);

        //    var function = kernel.CreateFunctionFromPrompt(promptTemplate, executionSetting, functionName, description);
        //    iWantToRun.Functions.Add(function);
        //    return (iWantToRun, function);
        //}


        #endregion
    }
}