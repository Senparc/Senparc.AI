using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Entities
{
    public class AgentKernelArguments : Dictionary<string, object?>, ISenparcKernelArguments
    {
        /// <summary>
        /// Replace parameters in the prompt
        /// </summary>
        /// <param name="prompt">Prompt</param>
        /// <param name="prefix">placeholder prefix</param>
        /// <param name="suffix">placeholder suffix</param>
        /// <returns></returns>
        public string ReplacePrompt(string prompt, string prefix = "${{", string suffix = "}}")
        {
            foreach (var item in this)
            {
                prompt = prompt.Replace($"{prefix}{item.Key}{suffix}", item.Value?.ToString());
            }
            return prompt;
        }
    }
}



