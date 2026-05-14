using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using Senparc.AI.AgentKernel.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Kernels
{
    public class AiKernel
    {
        public ConfigModel ConfigModel { get; set; }
        public object ChatClient { get; set; }//TODO:进行一次封装
        public object EmbeddingClient { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

        public ChatClientAgent ChatClientAgent { get; set; }

        public object EmbeddingGenerator { get; set; }

        public AgentSession? AgentSession { get; set; }

        public bool AgentInited { get; set; }

        public AiKernel(IServiceProvider serviceProvider, ConfigModel configModel, object chatClient, object embeddingModel)
        {
            this.ServiceProvider = serviceProvider;
            this.ChatClient = chatClient;
            this.ConfigModel = configModel;
            this.EmbeddingClient = embeddingModel;

            this.CreateAIAgent();
            this.CreateEmbeddingGenerator();
        }

        //TODO:Set Agent information

        internal void CreateAIAgent()
        {
            try
            {
                if (ConfigModel == ConfigModel.Unknown)
                {
                    throw new Exception("ConfigModel is required to create AIAgent");
                }

                if (ConfigModel != ConfigModel.Chat || ChatClient == null)
                {
                    return;
                }

                this.ChatClientAgent = ChatClient switch
                {
                    ChatClient c => c.AsAIAgent("You are a friendly assistant. Keep your answers brief", "SenparcAgent"),
                    OllamaChatClient c => c.AsAIAgent("You are a friendly assistant. Keep your answers brief", "SenparcAgent"),
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
            if (ConfigModel == ConfigModel.Unknown)
            {
                throw new Exception("ConfigModel is required to create EmbeddingGenerator");
            }

            if (ConfigModel != ConfigModel.TextEmbedding || EmbeddingClient == null)
            {
                return;
            }

            this.EmbeddingGenerator = EmbeddingClient switch
            {
                EmbeddingClient c => c.AsIEmbeddingGenerator(),//TODO: add defaultModelDimensions
                OllamaEmbeddingGenerator c => c,
                _ => throw new Exception("Unsupported EmbeddingClient type")
            };
        }

        public async Task<AgentResponse<T>> RunAsync<T>(string prompt, AgentSession agentSession = null)
        {
            if (ConfigModel != ConfigModel.Chat)
            {
                throw new Exception("RunAsync is only supported for Chat ConfigModel");
            }

            //TODO: Session 统一管理
            var session = agentSession ??= this.AgentSession;
            var result = await ChatClientAgent.RunAsync<T>(prompt, session);
            return result;
        }

        public async Task<AgentResponse> RunAsync(string prompt, AgentSession agentSession = null)
        {
            if (ConfigModel != ConfigModel.Chat)
            {
                throw new Exception("RunAsync is only supported for Chat ConfigModel");
            }

            var session = agentSession ??= this.AgentSession;

            var result = await ChatClientAgent.RunAsync(prompt, session);
            return result;
        }

        //public async Task<AgentResponse<T>> RunAsync<T>(AIFunction function, KernelArguments kernelArguments = null)
        //{
        //    if (ConfigModel != ConfigModel.Chat)
        //    {
        //        throw new Exception("RunAsync is only supported for Chat ConfigModel");
        //    }
        //}

        internal async Task<AgentResponse?> InvokeAsync(string prompt)
        {
            return await ChatClientAgent.RunAsync(prompt);
        }
    }
}
