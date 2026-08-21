using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Interfaces
{
    public interface ISenparcKernelArguments : IDictionary<string, object?>
    {
        /// <summary>
        /// Replace parameters in the prompt
        /// </summary>
        /// <param name="prompt">Prompt</param>
        /// <param name="prefix">placeholder prefix</param>
        /// <param name="suffix">placeholder suffix</param>
        /// <returns></returns>
        string ReplacePrompt(string prompt, string prefix = "${{", string suffix = "}}");
    }
}
