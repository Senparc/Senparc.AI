using Microsoft.SemanticKernel.Embeddings;
using Senparc.AI.Kernel.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Handlers
{
    public static partial class KernelConfigExtensions
    {
        #region Memory 相关

#pragma warning disable SKEXP0001
        public static IWantToRun MemorySaveInformation(this IWantToRun iWantToRun,
            string modelName,
            string collection,
            string text,
            string id,
            string? description = null,
            string? additionalMetadata = null,
            string azureDeployName = null,
            ITextEmbeddingGenerationService textEmbeddingGeneration = null,
            Microsoft.SemanticKernel.Kernel kernel = null,
            CancellationToken cancel = default)
        {
            var helper = iWantToRun.SemanticKernelHelper;
            var memory = helper.GetMemory(modelName, Senparc.AI.Config.SenparcAiSetting, null, azureDeployName, textEmbeddingGeneration);
            var task = helper.MemorySaveInformationAsync(memory, collection, text, id, description, additionalMetadata, kernel, cancel);
            helper.AddMemory(task);

            return iWantToRun;
        }

        public static IWantToRun MemorySaveReference(this IWantToRun iWantToRun,
               string modelName,
               string collection,
               string text,
               string externalId,
               string externalSourceName,
               string? description = null,
               string? additionalMetadata = null,
               string azureDeployName = null,
               ITextEmbeddingGenerationService textEmbeddingGeneration = null,
               Microsoft.SemanticKernel.Kernel kernel = null,
               CancellationToken cancel = default)
        {
            var helper = iWantToRun.SemanticKernelHelper;
            //var kernel = helper.GetKernel();
            var memory = helper.GetMemory(modelName, Senparc.AI.Config.SenparcAiSetting, null, azureDeployName, textEmbeddingGeneration);
            var task = helper.MemorySaveReferenceAsync(memory, collection, text, externalId, externalSourceName, description, additionalMetadata, kernel, cancel);
            helper.AddMemory(task);

            return iWantToRun;
        }

        public static IWantToRun MemoryStoreExexute(this IWantToRun iWantToRun)
        {
            var helper = iWantToRun.SemanticKernelHelper;
            helper.ExecuteMemory();
            return iWantToRun;
        }

        /// <summary>
        /// Memory 查询
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="collection">Collection to search</param>
        /// <param name="query">What to search for</param>
        /// <param name="limit">How many results to return</param>
        /// <param name="minRelevanceScore">Minimum relevance score, from 0 to 1, where 1 means exact match.</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>Memories found</returns>
        /// <returns></returns>
        public static async Task<SenaprcAiResult_MemoryQuery> MemorySearchAsync(this IWantToRun iWantToRun,
            string modelName,
            string memoryCollectionName,
            string query,
            int limit = 1,
            double minRelevanceScore = 0.7,
            bool withEmbeddings = false,
            string azureDeployName = null,
            ITextEmbeddingGenerationService textEmbeddingGeneration = null,
            Microsoft.SemanticKernel.Kernel kernel = null,
            CancellationToken cancel = default)
        {
            var helper = iWantToRun.SemanticKernelHelper;
            var memory = helper.GetMemory(modelName, Senparc.AI.Config.SenparcAiSetting, null, azureDeployName, textEmbeddingGeneration);
            var queryResult = memory.SearchAsync(memoryCollectionName, query, limit, minRelevanceScore, withEmbeddings, kernel, cancel);

            var aiResult = new SenaprcAiResult_MemoryQuery(iWantToRun, query)
            {
                MemoryQueryResult = queryResult,
            };
            return aiResult;
        }

        #endregion
    }
}
