using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Kernels
{
    public interface IAIKernelBuilder
    {
        ConfigModel ConfigModel { get; set; }
        object ChatClient { get; set; }
        object EmbeddingClient { get; set; }
        IServiceProvider ServiceProvider { get; set; }
        IServiceCollection Services { get; set; }
        string EmbeddingCollectionName { get; set; }
        int EmbeddingDimensions { get; set; }

        AiKernel Build();
    }

    public class AIKernelBuilder : IAIKernelBuilder
    {
        public IServiceCollection Services { get; set; } = new ServiceCollection();

        public IServiceProvider ServiceProvider { get; set; }

        public object ChatClient { get; set; }

        public object EmbeddingClient { get; set; }

        public ConfigModel ConfigModel { get; set; }
        public string EmbeddingCollectionName { get; set; }
        public int EmbeddingDimensions { get; set; }

        public AIKernelBuilder(ConfigModel configModel = ConfigModel.Unknown)
        {
            ConfigModel = configModel;
        }

        public static AIKernelBuilder CreateBuilder(ConfigModel configModel = ConfigModel.Unknown)
        {
            return new AIKernelBuilder(configModel);
        }

        public AiKernel Build()
        {
            ServiceProvider = Services.BuildServiceProvider();
            AiKernel aiKernel = new AiKernel(ServiceProvider, ConfigModel, ChatClient, EmbeddingClient);

            return aiKernel;
        }

    }
}
