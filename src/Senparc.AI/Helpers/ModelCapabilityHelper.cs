using System;
using System.Text.RegularExpressions;

namespace Senparc.AI.Helpers
{
    /// <summary>
    /// Determines API capability differences from the model name, such as GPT-5 and o-series reasoning models not supporting Temperature.
    /// </summary>
    public static class ModelCapabilityHelper
    {
        /// <summary>
        /// Matches models that do not support a custom Temperature: gpt-5*, o1*, o3*, and o4*.
        /// </summary>
        private static readonly Regex TemperatureUnsupportedModelRegex = new(
            @"^(?:gpt-5|o1|o3|o4)(?:$|[.\-_])",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Determines whether the model does not support including <c>temperature</c> and similar sampling parameters in a request.
        /// GPT-5 and later reasoning models and the o1/o3/o4 series allow only default values; sending a non-default value or the field itself may cause a 400 response.
        /// </summary>
        /// <param name="modelName">Chat or Completion model name, such as gpt-5.6-sol, gpt-5-mini, or o3-mini.</param>
        public static bool DoesNotSupportTemperature(string? modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                return false;
            }

            var name = modelName.Trim();

            // Support "provider/model" and deployment-name prefixes.
            var slashIndex = name.LastIndexOf('/');
            if (slashIndex >= 0 && slashIndex < name.Length - 1)
            {
                name = name.Substring(slashIndex + 1);
            }

            return TemperatureUnsupportedModelRegex.IsMatch(name);
        }

        /// <summary>
        /// Determines whether the model supports a custom Temperature.
        /// </summary>
        public static bool SupportsTemperature(string? modelName) => !DoesNotSupportTemperature(modelName);
    }
}
