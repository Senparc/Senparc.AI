using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI.Embeddings;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Senparc.AI.AgentKernel.KernelConfigExtensions
{
    public partial class KernelConfigExtensions
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
        public static async Task<ReadOnlyMemory<float>?> GetEmbeddingAsync(this IWantToRun iWantToRun, string text, object embeddingGenerationOptions = null, CancellationToken cancellationToken = default)
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
                if (embeddingGenerator is OllamaEmbeddingGenerator g)
                {
                    embeddingResult = (await g.GenerateAsync(new[] { text }, embeddingGenerationOptions as Microsoft.Extensions.AI.EmbeddingGenerationOptions, cancellationToken)).FirstOrDefault()?.Vector;
                }
                else if (embeddingGenerator is IEmbeddingGenerator<string, Embedding<float>> g2)
                {
                    embeddingResult = (await g2.GenerateAsync(text/* embeddingGenerationOptions as OpenAI.Embeddings.EmbeddingGenerationOptions, cancellationToken*/)).Vector;
                }
                else { 
                throw new Exception("Unsupported Embedding Generator type. Please use OllamaEmbeddingGenerator or IEmbeddingGenerator<string, Embedding<float>>.");
                }


                    return embeddingResult;
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
