using Microsoft.Extensions.AI;
using Senparc.AI.Helpers;
using System;

namespace Senparc.AI.AgentKernel.Helpers
{
    /// <summary>
    /// Sanitizes <see cref="ChatOptions"/> according to model capabilities to avoid sending sampling parameters such as Temperature to unsupported models.
    /// </summary>
    public static class ChatOptionsSanitizer
    {
        /// <summary>
        /// Sets related sampling parameters to null when the model does not support Temperature, preventing those fields from being submitted.
        /// </summary>
        /// <param name="chatOptions">The ChatOptions to sanitize; returns immediately when null.</param>
        /// <param name="modelName">The Chat model name.</param>
        /// <returns>Whether Temperature or another sampling parameter was removed.</returns>
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
                    $"[Debug] Model {modelName} does not support Temperature (current value={chatOptions.Temperature}); the parameter was ignored before submission.");
                chatOptions.Temperature = null;
                removed = true;
            }

            // Reasoning models in the same series usually do not support top_p, presence_penalty, or frequency_penalty either.
            if (chatOptions.TopP.HasValue)
            {
                Console.WriteLine(
                    $"[Debug] Model {modelName} does not support TopP (current value={chatOptions.TopP}); the parameter was ignored before submission.");
                chatOptions.TopP = null;
                removed = true;
            }

            if (chatOptions.PresencePenalty.HasValue)
            {
                Console.WriteLine(
                    $"[Debug] Model {modelName} does not support PresencePenalty (current value={chatOptions.PresencePenalty}); the parameter was ignored before submission.");
                chatOptions.PresencePenalty = null;
                removed = true;
            }

            if (chatOptions.FrequencyPenalty.HasValue)
            {
                Console.WriteLine(
                    $"[Debug] Model {modelName} does not support FrequencyPenalty (current value={chatOptions.FrequencyPenalty}); the parameter was ignored before submission.");
                chatOptions.FrequencyPenalty = null;
                removed = true;
            }

            return removed;
        }
    }
}
