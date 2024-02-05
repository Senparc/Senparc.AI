using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI
{
    public class DefaultSetting
    {
        public const string DEFAULT_SYSTEM_MESSAGE = @"ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.";

        /// <summary>
        /// 获取给 Chat 用的 Prompt
        /// </summary>
        /// <param name="systemMessage"></param>
        /// <param name="humanId"></param>
        /// <param name="robotId"></param>
        /// <param name="hisgoryArgName"></param>
        /// <param name="humanInputArgName"></param>
        /// <returns></returns>
        public static string GetPromptForChat(string systemMessage = DEFAULT_SYSTEM_MESSAGE, string humanId = "Human", string robotId = "ChatBot", string hisgoryArgName = "history", string humanInputArgName = "human_input")
        {
            return $@"{systemMessage}

{{{{${hisgoryArgName}}}}}
{humanId}: {{{{${humanInputArgName}}}}}
{robotId}:";
        }
    }
}
