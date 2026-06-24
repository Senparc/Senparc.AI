using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Kernels
{
    public interface IAIKernelBuilder
    {
        List<ConfigModel> ConfigModels { get; set; }
        object ChatClient { get; set; }
        object ImageClient { get; set; }
        object EmbeddingClient { get; set; }
        object SpeechToTextClient { get; set; }
        object TextToSpeechClient { get; set; }
        IServiceProvider ServiceProvider { get; set; }
        IServiceCollection Services { get; set; }
        string EmbeddingCollectionName { get; set; }
        Func<IEmbeddingGenerator, VectorStore> VectorStoreBuild { get; set; }
        void AddConfigModel(ConfigModel configModel);

        AiKernel Build(ISenparcAiSetting senparcAiSetting, ChatClientAgentOptions chatClientAgentOptions = null);
    }

    public class AIKernelBuilder : IAIKernelBuilder
    {
        public IServiceCollection Services { get; set; } = new ServiceCollection();

        public IServiceProvider ServiceProvider { get; set; }

        public object ChatClient { get; set; }

        public object ImageClient { get; set; }

        public object EmbeddingClient { get; set; }

        public object SpeechToTextClient { get; set; }

        public object TextToSpeechClient { get; set; }

        public List<ConfigModel> ConfigModels { get; set; }
        public string EmbeddingCollectionName { get; set; }

        public Func<IEmbeddingGenerator, VectorStore> VectorStoreBuild { get; set; }

        private AIKernelBuilder()
        {
            ConfigModels = new List<ConfigModel>();
        }

        public static AIKernelBuilder CreateBuilder()
        {
            return new AIKernelBuilder();
        }

        public void AddConfigModel(ConfigModel configModel)
        {
            ConfigModels.Add(configModel);
        }

        public AiKernel Build(ISenparcAiSetting senparcAiSetting, ChatClientAgentOptions chatClientAgentOptions = null)
        {
            ServiceProvider = Services.BuildServiceProvider();
            AiKernel aiKernel = new AiKernel(ServiceProvider, senparcAiSetting, ConfigModels.ToArray(), ChatClient, ImageClient, EmbeddingClient, SpeechToTextClient, TextToSpeechClient, EmbeddingCollectionName, chatClientAgentOptions);

            return aiKernel;
        }

    }
}
