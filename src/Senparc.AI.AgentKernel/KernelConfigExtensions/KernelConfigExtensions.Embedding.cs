using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OpenAI.Embeddings;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Senparc.AI.AgentKernel.Handlers
{
    public static partial class KernelConfigExtensions
    {
        /// <summary>
        /// Get Embedding Result
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="text"></param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="azureDeployName"></param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public static async Task<ReadOnlyMemory<float>> GetEmbeddingAsync(this IWantToRun iWantToRun, string text, object embeddingGenerationOptions = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var aiKernel = iWantToRun.Kernel;
                var embeddingGenerator = aiKernel.EmbeddingGenerator;
                if (embeddingGenerator == null)
                {
                    throw new Exception("Embedding Generator is not configured. Please configure it in the KernelConfig.");
                }

                ReadOnlyMemory<float>? embeddingResult = null;
                //if (embeddingGenerator is OllamaSharp OllamaEmbeddingGenerator g)
                //{
                //    embeddingResult = (await g.GenerateAsync(new[] { text }, embeddingGenerationOptions as Microsoft.Extensions.AI.EmbeddingGenerationOptions, cancellationToken)).FirstOrDefault()?.Vector;
                //}
                //else 
                if (embeddingGenerator is IEmbeddingGenerator<string, Embedding<float>> g2)
                {
                    embeddingResult = (await g2.GenerateAsync(text/* embeddingGenerationOptions as OpenAI.Embeddings.EmbeddingGenerationOptions, cancellationToken*/)).Vector;
                }
                else if (aiKernel.SenparcAiSetting.AiPlatform == AiPlatform.Ollama &&
                    aiKernel.EmbeddingClient is OllamaApiClient ollamaClient)
                {
                    ReadOnlyMemory<float> result = new ReadOnlyMemory<float>();

                    var embeddings = (await ollamaClient.EmbedAsync(text, cancellationToken)).Embeddings;

                    if (embeddings != null && embeddings.Count > 0)
                    {
                        result = new ReadOnlyMemory<float>(embeddings[0]);
                    }
                    embeddingResult = result;
                    //embeddingResult = (await (embeddingGenerator as OllamaEmbeddingGenerator).GenerateAsync(new[] { text }, embeddingGenerationOptions as OllamaEmbeddingGenerationOptions, cancellationToken)).FirstOrDefault()?.Vector;
                }
                else
                {
                    throw new Exception("Unsupported Embedding Generator type. Please use OllamaEmbeddingGenerator or IEmbeddingGenerator<string, Embedding<float>>.");
                }
                return embeddingResult ?? ReadOnlyMemory<float>.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Senparc.AIKernel Embedding 出错：");
                Console.WriteLine(ex);
                throw;
            }


        }
    }
}
