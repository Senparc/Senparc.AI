using Microsoft.Extensions.AI;
using Senparc.AI.Helpers;
using System;

namespace Senparc.AI.AgentKernel.Helpers
{
    /// <summary>
    /// 按模型能力清理 <see cref="ChatOptions"/>，避免向不支持的模型提交 Temperature 等采样参数。
    /// </summary>
    public static class ChatOptionsSanitizer
    {
        /// <summary>
        /// 若模型不支持 Temperature，则将相关采样参数置为 null（提交时不会出现该字段）。
        /// </summary>
        /// <param name="chatOptions">待清理的 ChatOptions；为 null 时直接返回</param>
        /// <param name="modelName">Chat 模型名</param>
        /// <returns>是否移除了 Temperature（或其它采样参数）</returns>
        public static bool SanitizeForModel(ChatOptions? chatOptions, string? modelName)
        {
            if (chatOptions == null || !ModelCapabilityHelper.DoesNotSupportTemperature(modelName))
            {
                return false;
            }

            var removed = false;

            if (chatOptions.Temperature.HasValue)
            {
                Console.WriteLine(
                    $"[调试] 模型 {modelName} 不支持 Temperature（当前值={chatOptions.Temperature}），提交前已忽略该参数");
                chatOptions.Temperature = null;
                removed = true;
            }

            // 同系列推理模型通常也不支持 top_p / presence_penalty / frequency_penalty
            if (chatOptions.TopP.HasValue)
            {
                Console.WriteLine(
                    $"[调试] 模型 {modelName} 不支持 TopP（当前值={chatOptions.TopP}），提交前已忽略该参数");
                chatOptions.TopP = null;
                removed = true;
            }

            if (chatOptions.PresencePenalty.HasValue)
            {
                Console.WriteLine(
                    $"[调试] 模型 {modelName} 不支持 PresencePenalty（当前值={chatOptions.PresencePenalty}），提交前已忽略该参数");
                chatOptions.PresencePenalty = null;
                removed = true;
            }

            if (chatOptions.FrequencyPenalty.HasValue)
            {
                Console.WriteLine(
                    $"[调试] 模型 {modelName} 不支持 FrequencyPenalty（当前值={chatOptions.FrequencyPenalty}），提交前已忽略该参数");
                chatOptions.FrequencyPenalty = null;
                removed = true;
            }

            return removed;
        }
    }
}
