using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Kernels
{
    public class ChatHistory : List<ChatMessage>
    {
        public void Add(ChatRole chatRole, string message)
        {
            base.Add(new ChatMessage(chatRole, message));
        }

        public void AddSystemMessage(string message)
        {
            Add(ChatRole.System, message);
        }


        public void AddUserMessage(string message)
        {
            Add(ChatRole.User, message);
        }

        public void AddAssistantMessage(string message)
        {
            Add(ChatRole.Assistant, message);
        }

        public void AddToolMessage(string message)
        {
            Add(ChatRole.Tool, message);
        }

    }
}
