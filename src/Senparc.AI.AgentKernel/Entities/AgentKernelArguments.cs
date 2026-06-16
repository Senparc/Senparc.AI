using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Entities
{
    public class AgentKernelArguments : Dictionary<string, object?>, ISenparcKernelArguments
    {
        /// <summary>
        /// 替换提示词中的参数
        /// </summary>
        /// <param name="prompt">提示词</param>
        /// <param name="prefix">占位符前缀</param>
        /// <param name="suffix">占位符后缀</param>
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



