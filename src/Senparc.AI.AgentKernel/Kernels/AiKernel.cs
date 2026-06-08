using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI.Chat;
using OpenAI.Embeddings;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Senparc.AI.AgentKernel.Kernels
{
    public class AiKernel
    {
        public ConfigModel[] ConfigModels { get; set; }
        public object ChatClient { get; set; }//TODO:进行一次封装
        public object ImageClient { get; set; }
        public object EmbeddingClient { get; set; }
        public string EmbeddingCollectionName { get; }
        public ChatClientAgentOptions ChatClientAgentOptions { get; set; }
        public ModelName ModelName { get; }
        public int EmbeddingDimensions { get; }
        public IServiceProvider ServiceProvider { get; set; }
        public ISenparcAiSetting SenparcAiSetting { get; }
        public ChatClientAgent ChatClientAgent { get; set; }

        public IEmbeddingGenerator EmbeddingGenerator { get; set; }//IEmbeddingGenerator<string, Embedding<float>>

        public AgentSession? AgentSession { get; set; }

        public bool AgentInited { get; set; }

        public AiKernel(IServiceProvider serviceProvider, ISenparcAiSetting senparcAiSetting, ConfigModel[] configModels, object chatClient, object imageClient, object embeddingClient, string embeddingCollectionName, ChatClientAgentOptions chatClientAgentOptions)
        {
            this.ServiceProvider = serviceProvider;
            this.SenparcAiSetting = senparcAiSetting;
            this.ChatClient = chatClient;
            this.ImageClient = imageClient;
            this.ConfigModels = configModels;
            this.EmbeddingClient = embeddingClient;
            this.EmbeddingCollectionName = embeddingCollectionName;
            this.ChatClientAgentOptions = chatClientAgentOptions;
            this.ModelName = senparcAiSetting.ModelName;
            this.EmbeddingDimensions = ModelName.EmbeddingDimensions ?? 0;
            this.CreateAIAgent();
            this.CreateEmbeddingGenerator();
        }

        //TODO:Set Agent information

        internal void CreateAIAgent()
        {
            try
            {
                if (ConfigModels == null || ConfigModels.Length == 0)
                {
                    throw new Exception("ConfigModel is required to create AIAgent");
                }

                if (!ConfigModels.Contains(ConfigModel.Chat))
                {
                    return;
                }

                ChatClientAgentOptions ??= new ChatClientAgentOptions()
                {
                    Id = $"SenparcAIAgent-{Guid.NewGuid().ToString("n")}",
                    Name = "SenparcAgent",
                    Description = "You are a friendly assistant. Keep your answers brief"
                };

                this.ChatClientAgent = ChatClient switch
                {
                    ChatClient c => c.AsAIAgent(ChatClientAgentOptions),
                    OllamaApiClient c => c.AsAIAgent(ChatClientAgentOptions),
                    _ => throw new Exception("Unsupported ChatClient type")
                };

                AgentInited = true;
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Set AgentSesssion
        /// </summary>
        /// <param name="session">use default method to create AgentSession if set null</param>
        /// <returns></returns>
        public async Task<AgentSession> SetAgentSessionAsync(AgentSession session)
        {
            AgentSession newSession = session ?? await ChatClientAgent.CreateSessionAsync();
            this.AgentSession = newSession;
            return newSession;
        }

        private void CreateEmbeddingGenerator()
        {
            if (ConfigModels == null || ConfigModels.Length == 0)
            {
                throw new Exception("ConfigModel is required to create AIAgent");
            }

            if (!ConfigModels.Contains(ConfigModel.TextEmbedding))
            {
                return;
            }

            if (EmbeddingCollectionName.IsNullOrEmpty())
            {
                throw new Exception("EmbeddingCollectionName is required to create EmbeddingGenerator");
            }

            if (EmbeddingDimensions == 0)
            {
                throw new Exception("EmbeddingDimensions is required to create EmbeddingGenerator");
            }

            this.EmbeddingGenerator = EmbeddingClient switch
            {
                EmbeddingClient c => c.AsIEmbeddingGenerator(EmbeddingDimensions),//TODO: add defaultModelDimensions
                OllamaApiClient c => c,
                _ => throw new Exception("Unsupported EmbeddingClient type")
            };
        }

        //public async Task<AgentResponse<T>> RunChatAsync<T>(string prompt, AgentSession? agentSession = null)
        //{
        //    EnsureChatConfigModel();
        //    //TODO: Session 统一管理
        //    var session = agentSession ??= AgentSession;
        //    var result = await ChatClientAgent.RunAsync<T>(prompt, session);
        //    return result;
        //}

        //public async Task<AgentResponse> RunChatAsync(string prompt, AgentSession? agentSession = null)
        //{
        //    EnsureChatConfigModel();
        //    var session = agentSession ??= AgentSession;
        //    return await ChatClientAgent.RunAsync(prompt, session);
        //}

        //public async Task<AgentResponse> RunChatAsync(AgentSession agentSession = null, ChatClientAgentRunOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    EnsureChatConfigModel();
        //    var session = agentSession ??= AgentSession;
        //    return await ChatClientAgent.RunAsync(session, options, cancellationToken);
        //}

        //public async Task<AgentResponse> RunChatAsync(IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages, AgentSession? agentSession = null, ChatClientAgentRunOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    EnsureChatConfigModel();
        //    var session = agentSession ??= AgentSession;
        //    return await ChatClientAgent.RunAsync(messages, session, options, cancellationToken);
        //}

        ///// <summary>
        ///// 流式运行 Agent（MAF <see cref="ChatClientAgent.RunStreamingAsync"/>）
        ///// </summary>
        //public IAsyncEnumerable<AgentResponseUpdate> RunChatStreamingAsync(string prompt, AgentSession? agentSession = null, ChatClientAgentRunOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    EnsureChatConfigModel();
        //    var session = agentSession ?? AgentSession;
        //    return ChatClientAgent.RunStreamingAsync(prompt, session, options, cancellationToken);
        //}

        //public IAsyncEnumerable<AgentResponseUpdate> RunChatStreamingAsync(AgentSession? agentSession = null, ChatClientAgentRunOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    EnsureChatConfigModel();
        //    var session = agentSession ?? AgentSession;
        //    return ChatClientAgent.RunStreamingAsync(session, options, cancellationToken);
        //}

        //public IAsyncEnumerable<AgentResponseUpdate> RunChatStreamingAsync(IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages, AgentSession? agentSession = null, ChatClientAgentRunOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    EnsureChatConfigModel();
        //    var session = agentSession ?? AgentSession;
        //    return ChatClientAgent.RunStreamingAsync(messages, session, options, cancellationToken);
        //}

        //public IAsyncEnumerable<AgentResponseUpdate> RunChatStreamingAsync(Microsoft.Extensions.AI.ChatMessage message, AgentSession? agentSession = null, ChatClientAgentRunOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    EnsureChatConfigModel();
        //    var session = agentSession ?? AgentSession;
        //    return ChatClientAgent.RunStreamingAsync(message, session, options, cancellationToken);
        //}

        private void EnsureChatConfigModel()
        {
            if (!ConfigModels.Contains(ConfigModel.Chat))
            {
                throw new Exception("Run is only supported for Chat ConfigModel");
            }
        }

        //public async Task<AgentResponse<T>> RunAsync<T>(AIFunction function, AgentKernelArguments kernelArguments = null)
        //{
        //    if (ConfigModel != ConfigModel.Chat)
        //    {
        //        throw new Exception("RunAsync is only supported for Chat ConfigModel");
        //    }
        //}

        internal async Task<AgentResponse?> InvokeChatAsync(string prompt, AgentSession session = null)
        {
            return await ChatClientAgent.RunAsync(prompt, session ?? AgentSession);
        }

        internal async Task<AgentResponse<T>> InvokeChatAsync<T>(string prompt, AgentSession session = null)
        {
            return await ChatClientAgent.RunAsync<T>(prompt, session ?? AgentSession);
        }

        internal IAsyncEnumerable<AgentResponseUpdate> InvokeChatStreamingAsync(string prompt, AgentSession session = null)
        {
            return ChatClientAgent.RunStreamingAsync(prompt, session ?? AgentSession);
        }
    }
}
