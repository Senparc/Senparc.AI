using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI
{
    public class DefaultSetting
    {
        /// <summary>
        /// 默认给 Chat 用的 Prompt
        /// </summary>
        public static string DefaultPromptForChat { get; set; } = @"
ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

{{$history}}
Human: {{$human_input}}
ChatBot:";
    }
}
