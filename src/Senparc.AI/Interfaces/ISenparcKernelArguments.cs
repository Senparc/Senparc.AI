using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Interfaces
{
    public interface ISenparcKernelArguments : IDictionary<string, object?>
    {
        /// <summary>
        /// 替换提示词中的参数
        /// </summary>
        /// <param name="prompt">提示词</param>
        /// <param name="prefix">占位符前缀</param>
        /// <param name="suffix">占位符后缀</param>
        /// <returns></returns>
        string ReplacePrompt(string prompt, string prefix = "${{", string suffix = "}}");
    }
}
