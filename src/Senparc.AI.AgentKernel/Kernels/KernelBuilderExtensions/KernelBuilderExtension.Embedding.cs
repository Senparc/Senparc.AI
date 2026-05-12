using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions
{
    public static partial class KernelBuilderExtension
    {
        #region AddEmbedding

        public static EmbeddingClient AddOpenAIEmbedding(this IAIKernelBuilder kernelBuilder,
            string apiKey, string modelName)
        {
            return new EmbeddingClient(modelName, apiKey);
            //var embeddingClient = new OpenAIClient(apiKey).GetEmbeddingClient(modelName);
        }

        public static EmbeddingClient AddAzureOpenAIEmbedding(this IAIKernelBuilder kernelBuilder,
            Uri endpoint, ApiKeyCredential credential, AzureOpenAIClientOptions options, string azureDeploymentName)
        {
            return new AzureOpenAIClient(endpoint, credential, options).GetEmbeddingClient(azureDeploymentName);
        }

        public static EmbeddingClient AddNeuCharAIEmbedding(this IAIKernelBuilder kernelBuilder,
          Uri endpoint, ApiKeyCredential credential, AzureOpenAIClientOptions options, string modelName)
        {
            return AddAzureOpenAIEmbedding(kernelBuilder, endpoint, credential, options, modelName);
        }

        public static OllamaEmbeddingGenerator AddOllamaEmbedding(this IAIKernelBuilder kernelBuilder,
       string endpoint, string modelName)
        {
            return new OllamaEmbeddingGenerator(endpoint, modelName);
        }

        #endregion

        #region Generate Embedding

        public static Task GenerateOpenAIEmbeddingAsync(List<string> content) { 
        
        }

        #endregion
    }
}
