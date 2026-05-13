using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using OpenAI.Embeddings;
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

        public AiKernel(IServiceProvider serviceProvider, ConfigModel configModel, object chatClient, object embeddingModel)
        {
            this.ServiceProvider = serviceProvider;
            this.ChatClient = chatClient;
            this.ConfigModel = configModel;
            this.EmbeddingClient = embeddingModel;

            this.CreateAIAgent();
            this.CreateEmbeddingGenerator();
        }

        private void CreateAIAgent()
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
                ChatClient c => c.AsAIAgent(),
                OllamaChatClient c => c.AsAIAgent(),
                _ => throw new Exception("Unsupported ChatClient type")
            };
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

        public async Task<AgentResponse<T>> RunAsync<T>(string prompt)
        {
            if (ConfigModel != ConfigModel.Chat)
            {
                throw new Exception("RunAsync is only supported for Chat ConfigModel");
            }

            //TODO: Session 统一管理
            var result = await ChatClientAgent.RunAsync<T>(prompt);
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
