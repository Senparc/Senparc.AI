using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Senparc.CO2NET.Extensions;
using Microsoft.Extensions.Http.Logging;
using System.Net.Http;
//using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using System.Net;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Runtime.CompilerServices;
using Senparc.AI.Entities.Keys;
using Senparc.CO2NET;
using RTools_NTS.Util;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

// Memory functionality is experimental
#pragma warning disable SKEXP0003, SKEXP0011, SKEXP0052, SKEXP0020, SKEXP0012, SKEXP0001

namespace Senparc.AI.Kernel.Helpers
{

    public partial class SemanticKernelHelper
    {
        /* Config* method rules:
        1. Related methods are lower-level call methods and directly use module interfaces such as Semantic Kernel
        2. All modelName and deploymentName values are passed as strings. If left empty, they are read automatically from SenparcAiSetting.
       */



        #region Config


        /// <summary>
        /// Set Kernel and configure the TextCompletion model
        /// </summary>
        /// <param name="userId">User ID, used to prevent API abuse</param>
        /// <param name="modelName">Model name modelId</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="kernelBuilder"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public IKernelBuilder ConfigChat(string userId, string modelName = null, ISenparcAiSetting senparcAiSetting = null,
            IKernelBuilder? kernelBuilder = null, string deploymentName = null)
        {
            senparcAiSetting ??= Senparc.AI.Config.SenparcAiSetting;
            modelName ??= senparcAiSetting.ModelName.Chat;
            deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

            var serviceId = GetServiceId(userId, modelName);
            var aiPlatForm = senparcAiSetting.AiPlatform;

            //TODO Need to check Kernel.TextCompletionServices.ContainsKey(serviceId). If it already exists, do not add it again.

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // The previous method has been marked obsolete by SK. Changed to the method recommended by SK.
            kernelBuilder ??= Microsoft.SemanticKernel.Kernel.CreateBuilder();

            // use `senparcAiSetting` instead of using `AiSetting` from the config file by default
            _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAIChatCompletion(modelName,
                        apiKey: senparcAiSetting.ApiKey,
                        orgId: senparcAiSetting.OrganizationId,
                        httpClient: _httpClient),
                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIChatCompletion(
                        deploymentName: deploymentName,
                        modelId: modelName,
                        endpoint: senparcAiSetting.AzureEndpoint,
                        apiKey: senparcAiSetting.ApiKey,
                        httpClient: _httpClient),
                AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAIChatCompletion(
                        deploymentName: deploymentName,
                        modelId: modelName,
                        endpoint: senparcAiSetting.NeuCharEndpoint,
                        apiKey: senparcAiSetting.ApiKey,
                        httpClient: _httpClient),
                AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceTextGeneration(
                        model: modelName,
                        apiKey: null,
                        endpoint: new Uri(senparcAiSetting.HuggingFaceEndpoint ?? throw new SenparcAiException("HuggingFace requires Endpoint")),
                        serviceId: serviceId,
                        httpClient: _httpClient),
                AiPlatform.FastAPI => kernelBuilder.AddFastAPIChatCompletion(
                        modelId: modelName,
                        apiKey: senparcAiSetting.ApiKey,
                        orgId: senparcAiSetting.OrganizationId,
                        endpoint: senparcAiSetting.FastAPIEndpoint,
                        serviceId: null
                    ),
                AiPlatform.Ollama => kernelBuilder.AddOllamaChatCompletion(
                        modelId: modelName,
                        endpoint: new Uri(senparcAiSetting.OllamaEndpoint),
                        serviceId: null
                    ),
                //DeepSeek uses the same request format as OpenAI
                AiPlatform.DeepSeek => kernelBuilder.AddOpenAIChatCompletion(modelName,
                        endpoint: new Uri(senparcAiSetting.Endpoint),
                        apiKey: senparcAiSetting.ApiKey,
                        orgId: senparcAiSetting.OrganizationId,
                        httpClient: _httpClient),

                _ => throw new SenparcAiException($"ConfigChat does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// Set Kernel and configure the TextCompletion model
        /// </summary>
        /// <param name="userId">User ID, used to prevent API abuse</param>
        /// <param name="modelName">Model name modelId</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="kernelBuilder"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public IKernelBuilder ConfigTextCompletion(string userId, string modelName = null, ISenparcAiSetting senparcAiSetting = null,
            IKernelBuilder? kernelBuilder = null, string deploymentName = null)
        {
            senparcAiSetting ??= Senparc.AI.Config.SenparcAiSetting;
            modelName ??= senparcAiSetting.ModelName.TextCompletion;
            deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

            var serviceId = GetServiceId(userId, modelName);
            var aiPlatForm = senparcAiSetting.AiPlatform;

            kernelBuilder ??= Microsoft.SemanticKernel.Kernel.CreateBuilder();

            //TODO Need to check Kernel.TextCompletionServices.ContainsKey(serviceId). If it already exists, do not add it again.

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // The previous method has been marked obsolete by SK. Changed to the method recommended by SK.

            // use `senparcAiSetting` instead of using `AiSetting` from the config file by default

            _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAIChatCompletion(modelName,
                        apiKey: senparcAiSetting.ApiKey,
                        orgId: senparcAiSetting.OrganizationId,
                        httpClient: _httpClient),
                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIChatCompletion(
                        deploymentName: deploymentName,
                        modelId: modelName,
                        endpoint: senparcAiSetting.Endpoint,
                        apiKey: senparcAiSetting.ApiKey,
                        httpClient: _httpClient),
                AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAIChatCompletion(
                        deploymentName: deploymentName,
                        modelId: modelName,
                        endpoint: senparcAiSetting.Endpoint,
                        apiKey: senparcAiSetting.ApiKey,
                        httpClient: _httpClient),
                AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceTextGeneration(
                        model: modelName,
                        apiKey: null,
                        endpoint: new Uri(senparcAiSetting.Endpoint ?? throw new SenparcAiException("HuggingFace requires Endpoint")),
                        serviceId: null,
                        httpClient: _httpClient),
                AiPlatform.FastAPI => kernelBuilder.AddFastAPIChatCompletion(
                        modelId: modelName,
                        apiKey: senparcAiSetting.ApiKey,
                        orgId: senparcAiSetting.OrganizationId,
                        endpoint: senparcAiSetting.FastAPIEndpoint,
                        serviceId: null
                    ),
                AiPlatform.Ollama => kernelBuilder.AddOllamaTextGeneration(
                        modelId: modelName,
                        endpoint: new Uri(senparcAiSetting.OllamaEndpoint),
                        serviceId: null
                    ),
                AiPlatform.DeepSeek => kernelBuilder.AddOpenAIChatCompletion(modelName,
                        endpoint: new Uri(senparcAiSetting.Endpoint),
                        apiKey: senparcAiSetting.ApiKey,
                        orgId: senparcAiSetting.OrganizationId,
                        httpClient: _httpClient),
                _ => throw new SenparcAiException($"ConfigTextCompletion does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// Set Kernel and configure the TextCompletion model
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="kernelBuilder"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public IKernelBuilder ConfigTextEmbeddingGeneration(string userId, string modelName = null, ISenparcAiSetting senparcAiSetting = null, IKernelBuilder? kernelBuilder = null, string deploymentName = null)
        {
            //kernel ??= GetKernel();

            senparcAiSetting ??= this.AiSetting;
            modelName ??= senparcAiSetting.ModelName.Embedding;
            deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

            var serviceId = GetServiceId(userId, modelName);
            var aiPlatForm = senparcAiSetting.AiPlatform;
            //TODO Need to check Kernel.TextCompletionServices.ContainsKey(serviceId). If it already exists, do not add it again.

            //TODO:Builder should not be recreated

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // The previous method has been marked obsolete by SK. Changed to the method recommended by SK.
            kernelBuilder ??= Microsoft.SemanticKernel.Kernel.CreateBuilder();

            // use `senparcAiSetting` instead of using `AiSetting` from the config file by default
            _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAITextEmbeddingGeneration(
                    modelId: modelName,
                    apiKey: senparcAiSetting.ApiKey,
                    orgId: senparcAiSetting.OrganizationId,
                    httpClient: _httpClient),

                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
                    deploymentName: deploymentName,
                    endpoint: senparcAiSetting.Endpoint,
                    apiKey: senparcAiSetting.ApiKey,
                    modelId: modelName,
                    httpClient: _httpClient),

                AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
                    deploymentName: deploymentName,
                    endpoint: senparcAiSetting.Endpoint,
                    apiKey: senparcAiSetting.ApiKey,
                    modelId: modelName,
                    httpClient: _httpClient),

                AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceTextEmbeddingGeneration(
                    model: modelName,
                    endpoint: new Uri(senparcAiSetting.Endpoint ?? throw new SenparcAiException("HuggingFace requires Endpoint")),
                    httpClient: _httpClient),

                AiPlatform.Ollama => kernelBuilder.AddOllamaTextEmbeddingGeneration(
                    modelId: modelName,
                    endpoint: new Uri(senparcAiSetting.OllamaEndpoint)),

                _ => throw new SenparcAiException($"ConfigTextEmbeddingGeneration does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
            };

            //TODO:Test repeated additions
            //KernelBuilder = builder;
            return kernelBuilder;
        }

        /// <summary>
        /// Set the DallE interface. OpenAI credentials are forced by default.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="kernelBuilder"></param>
        /// <param name="azureModeId">AzureOpenAI model name</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="azureDallEDepploymentName">AzureAI DallE model deployment name</param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public IKernelBuilder ConfigImageGeneration(string userId, IKernelBuilder? kernelBuilder = null, string azureModeId = null, ISenparcAiSetting senparcAiSetting = null, string azureDallEDepploymentName = null)
        {
            senparcAiSetting ??= this.AiSetting;

            var serviceId = GetServiceId(userId, "image-generation");
            var aiPlatForm = senparcAiSetting.AiPlatform;

            //TODO:Builder should not be recreated
            kernelBuilder ??= Microsoft.SemanticKernel.Kernel.CreateBuilder();

            _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAITextToImage(
                    apiKey: senparcAiSetting.ApiKey,
                    orgId: senparcAiSetting.OrganizationId,
                    httpClient: _httpClient),

                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAITextToImage(
                    deploymentName: azureDallEDepploymentName,
                    endpoint: senparcAiSetting.Endpoint,
                    apiKey: senparcAiSetting.ApiKey,
                    modelId: azureModeId,
                    httpClient: _httpClient),

                AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAITextToImage(
                    deploymentName: azureDallEDepploymentName,
                    endpoint: senparcAiSetting.Endpoint,
                    apiKey: senparcAiSetting.ApiKey,
                    modelId: azureModeId,
                    httpClient: _httpClient),

                _ => throw new SenparcAiException($"ConfigImageGeneration does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// Set the Whisper speech-to-text interface
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="kernelBuilder"></param>
        /// <param name="modelId">Model name. Defaults to whisper</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="deploymentName">Azure deployment name</param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public IKernelBuilder ConfigAudioToText(string userId, IKernelBuilder? kernelBuilder = null, string modelId = null, ISenparcAiSetting senparcAiSetting = null, string deploymentName = null)
        {
            senparcAiSetting ??= this.AiSetting;
            modelId ??= "whisper"; // Use whisper by default model

            var serviceId = GetServiceId(userId, "audio-to-text");
            var aiPlatForm = senparcAiSetting.AiPlatform;

            kernelBuilder ??= Microsoft.SemanticKernel.Kernel.CreateBuilder();

            _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAIAudioToText(
                    modelId: modelId,
                    apiKey: senparcAiSetting.ApiKey,
                    orgId: senparcAiSetting.OrganizationId,
                    httpClient: _httpClient),

                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIAudioToText(
                    deploymentName: deploymentName ?? modelId,
                    endpoint: senparcAiSetting.Endpoint,
                    apiKey: senparcAiSetting.ApiKey,
                    httpClient: _httpClient),

                AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAIAudioToText(
                    deploymentName: deploymentName ?? modelId,
                    endpoint: senparcAiSetting.Endpoint,
                    apiKey: senparcAiSetting.ApiKey,
                    httpClient: _httpClient),

                _ => throw new SenparcAiException($"ConfigAudioToText does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// Set the TTS text-to-speech interface
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="kernelBuilder"></param>
        /// <param name="modelId">Model name. Defaults to tts. Other supported values include tts-hd.</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="deploymentName">Azure deployment name</param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public IKernelBuilder ConfigTextToAudio(string userId, IKernelBuilder? kernelBuilder = null, string modelId = null, ISenparcAiSetting senparcAiSetting = null, string deploymentName = null)
        {
            senparcAiSetting ??= this.AiSetting;
            modelId ??= "tts"; // Use tts by default model

            var serviceId = GetServiceId(userId, "text-to-audio");
            var aiPlatForm = senparcAiSetting.AiPlatform;

            kernelBuilder ??= Microsoft.SemanticKernel.Kernel.CreateBuilder();

            _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAITextToAudio(
                    modelId: modelId,
                    apiKey: senparcAiSetting.ApiKey,
                    orgId: senparcAiSetting.OrganizationId,
                    httpClient: _httpClient),

                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAITextToAudio(
                    deploymentName: deploymentName ?? modelId,
                    endpoint: senparcAiSetting.Endpoint,
                    apiKey: senparcAiSetting.ApiKey,
                    httpClient: _httpClient),

                AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAITextToAudio(
                    deploymentName: deploymentName ?? modelId,
                    endpoint: senparcAiSetting.Endpoint,
                    apiKey: senparcAiSetting.ApiKey,
                    httpClient: _httpClient),

                _ => throw new SenparcAiException($"ConfigTextToAudio does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// Get Embedding Result
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="text"></param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="azureDeployName"></param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public async Task<ReadOnlyMemory<float>> GetEmbeddingAsync(string modelName, string text, ISenparcAiSetting senparcAiSetting = null, string azureDeployName = null)
        {
            senparcAiSetting ??= this.AiSetting;
            var aiPlatForm = senparcAiSetting.AiPlatform;

            var embeddingService = aiPlatForm switch
            {
                //AiPlatform.OpenAI =>  new OpenAITextEmbeddingGenerationService(
                //                apiKey: senparcAiSetting.ApiKey,
                //                httpClient: _httpClient,
                //                modelId: modelName,
                //                loggerFactory: loggerFactory
                //           );
                // ),

                //memoryBuilder.WithAzureOpenAITextEmbeddingGeneration(
                //    modelId: modelName,
                //    apiKey: senparcAiSetting.ApiKey,
                //    orgId: senparcAiSetting.OrganizationId,
                //    httpClient: _httpClient),
                AiPlatform.AzureOpenAI => new AzureOpenAITextEmbeddingGenerationService(
                           deploymentName: azureDeployName ?? modelName,
                           endpoint: senparcAiSetting.Endpoint,
                                apiKey: senparcAiSetting.ApiKey,
                           httpClient: _httpClient,
                                modelId: modelName,
                                loggerFactory: loggerFactory
                           ),
                AiPlatform.NeuCharAI => new AzureOpenAITextEmbeddingGenerationService(
                            deploymentName: azureDeployName ?? modelName,
                           endpoint: senparcAiSetting.Endpoint,
                                apiKey: senparcAiSetting.ApiKey,
                           httpClient: _httpClient,
                                modelId: modelName,
                                loggerFactory: loggerFactory
                           ),
                //AiPlatform.HuggingFace => memoryBuilder.WithTextEmbeddingGeneration(
                //    (loggerFactory, httpClient) =>
                //    {
                //        return new AzureOpenAITextEmbeddingGenerationService(
                //        deploymentName: azureDeployName,
                //             endpoint: senparcAiSetting.Endpoint,
                //             apiKey: senparcAiSetting.ApiKey,
                //        httpClient: _httpClient,
                //             modelId: modelName,
                //             loggerFactory: loggerFactory
                //        );
                //    }),

                //AiPlatform.Ollama => memoryBuilder.WithTextEmbeddingGeneration((loggerFactory, httpClient) =>
                //{
                //    return new OllamaTextEmbeddingGenerationService(
                //         endpoint: new Uri(senparcAiSetting.Endpoint),
                //         modelId: modelName,
                //         loggerFactory: loggerFactory
                //    );
                //}),

                _ => throw new SenparcAiException($"GetEmbedding does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
            };

            //var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
            try
            {
                var embeddingResult = await embeddingService.GenerateEmbeddingAsync(text, _kernel);

                return embeddingResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Senparc.AIKernel Embedding error:");
                Console.WriteLine(ex);
                throw;
            }


        }

        #region Memory related

        ISemanticTextMemory _textMemory = null;//TODO:Adapt multiple different requests

        /// <summary>
        /// Try to get the ISemanticTextMemory object
        /// </summary>
        /// <returns></returns>
        /// <exception cref="SenparcAiException">Thrown when ISemanticTextMemory is not set</exception>
        public ISemanticTextMemory? TryGetMemory()
        {
            if (_textMemory == null)
            {
                throw new SenparcAiException("_textMemory is not set!");
            }
            return _textMemory;
        }

        /// <summary>
        /// Get the ISemanticTextMemory object
        /// </summary>
        /// <returns></returns>
        //[Obsolete("This method has been abandoned by SK. Original text: Memory functionality will be placed in separate Microsoft.SemanticKernel.Plugins.Memory package. This will be removed in a future release. See sample dotnet/samples/KernelSyntaxExamples/Example14_SemanticMemory.cs in the semantic-kernel repository.")]
        [Obsolete]
        public ISemanticTextMemory? GetMemory(string modelName, ISenparcAiSetting senparcAiSetting,
            IKernelBuilder? kernelBuilder, string azureDeployName = null, ITextEmbeddingGenerationService textEmbeddingGeneration = null)
        {
            if (_textMemory == null)
            {
                senparcAiSetting ??= this.AiSetting;
                var aiPlatForm = senparcAiSetting.AiPlatform;

                var memoryBuilder = new MemoryBuilder();
                memoryBuilder.WithHttpClient(_httpClient);



                _ = aiPlatForm switch
                {
                    AiPlatform.OpenAI => memoryBuilder.WithTextEmbeddingGeneration(
                           (loggerFactory, httpClient) =>
                           {
                               return new OpenAITextEmbeddingGenerationService(
                                    apiKey: senparcAiSetting.ApiKey,
                                    httpClient: _httpClient,
                                    modelId: modelName,
                                    loggerFactory: loggerFactory
                               );
                           }
                     ),

                    //memoryBuilder.WithAzureOpenAITextEmbeddingGeneration(
                    //    modelId: modelName,
                    //    apiKey: senparcAiSetting.ApiKey,
                    //    orgId: senparcAiSetting.OrganizationId,
                    //    httpClient: _httpClient),
                    AiPlatform.AzureOpenAI => memoryBuilder.WithTextEmbeddingGeneration(
                           (loggerFactory, httpClient) =>
                           {
                               return new AzureOpenAITextEmbeddingGenerationService(
                                    deploymentName: azureDeployName,
                                    endpoint: senparcAiSetting.Endpoint,
                                    apiKey: senparcAiSetting.ApiKey,
                                    httpClient: _httpClient,
                                    modelId: modelName,
                                    loggerFactory: loggerFactory
                               );
                           }
                    ),

                    AiPlatform.HuggingFace => memoryBuilder.WithTextEmbeddingGeneration(
                        (loggerFactory, httpClient) =>
                        {
                            return new AzureOpenAITextEmbeddingGenerationService(
                                 deploymentName: azureDeployName,
                                 endpoint: senparcAiSetting.Endpoint,
                                 apiKey: senparcAiSetting.ApiKey,
                                 httpClient: _httpClient,
                                 modelId: modelName,
                                 loggerFactory: loggerFactory
                            );
                        }),

                    AiPlatform.Ollama => memoryBuilder.WithTextEmbeddingGeneration((loggerFactory, httpClient) =>
                    {
                        return new OllamaTextEmbeddingGenerationService(
                             endpoint: new Uri(senparcAiSetting.Endpoint),
                             modelId: modelName,
                             loggerFactory: loggerFactory
                        );
                    }),

                    _ => throw new SenparcAiException($"GetMemory does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
                };


                memoryBuilder.WithMemoryStore(new VolatileMemoryStore());
                //.WithMemoryStore(new AzureAISearchMemoryStore(senparcAiSetting.AzureEndpoint, senparcAiSetting.ApiKey))

                _textMemory = memoryBuilder.Build();
            }


            return _textMemory;
        }

        #endregion


        /// <summary>
        /// Save some information into the semantic memory, keeping only a reference to the source information.
        /// </summary>
        /// <param name="memory">ISemanticTextMemory object</param>
        /// <param name="collection">Collection where to save the information</param>
        /// <param name="text">Information to save</param>
        /// <param name="externalId">Unique identifier, e.g. URL or GUID to the original source</param>
        /// <param name="externalSourceName">Name of the external service, e.g. "MSTeams", "GitHub", "WebSite", "Outlook IMAP", etc.</param>
        /// <param name="description">Optional description</param>
        /// <param name="additionalMetadata"></param>
        /// <param name="kernel">Kernel</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns></returns>
        public async Task MemorySaveReferenceAsync(ISemanticTextMemory memory,
            string collection,
            string text,
            string externalId,
            string externalSourceName,
            string? description = null,
            string? additionalMetadata = null,
            Microsoft.SemanticKernel.Kernel? kernel = null,
            CancellationToken cancel = default)
        {
            await memory.SaveReferenceAsync(collection, text, externalId, externalSourceName, description,
                additionalMetadata, kernel ?? GetKernel(), cancel);
        }

        /// <summary>
        /// Save some information into the semantic memory, keeping a copy of the source information.
        /// </summary>
        /// <param name="memory">ISemanticTextMemory object</param>
        /// <param name="collection">Collection where to save the information</param>
        /// <param name="id">Unique identifier</param>
        /// <param name="text">Information to save</param>
        /// <param name="description">Optional description</param>
        /// <param name="additionalMetadata"></param>
        /// <param name="kernel">Kernel</param>
        /// <param name="cancel">Cancellation token</param>        /// <returns></returns>
        public async Task MemorySaveInformationAsync(ISemanticTextMemory memory,
            string collection,
            string text,
            string id,
            string? description = null,
            string? additionalMetadata = null,
            Microsoft.SemanticKernel.Kernel? kernel = null,
            CancellationToken cancel = default)
        {
            await memory.SaveInformationAsync(collection, text, id, description, additionalMetadata, kernel ?? GetKernel(), cancel);
        }

        /// <summary>
        /// Add Memory operation
        /// </summary>
        /// <param name="task"></param>
        public void AddMemory(Task task)
        {
            _memoryExecuteList.Add(task);
        }

        /// <summary>
        /// Execute Memory operation and wait
        /// </summary>
        public void ExecuteMemory()
        {
            foreach (var task in _memoryExecuteList)
            {
                Task.Run(() => task);
            }

            Task.WaitAll(_memoryExecuteList.ToArray());
            _memoryExecuteList.Clear();
        }

        #endregion
    }
}

#pragma warning restore SKEXP0003, SKEXP0011, SKEXP0052, SKEXP0020

