using System;
using System.Text.RegularExpressions;

namespace Senparc.AI.Helpers
{
    /// <summary>
    /// 根据模型名称判断 API 能力差异（如 GPT-5 / o 系列推理模型不支持 Temperature）。
    /// </summary>
    public static class ModelCapabilityHelper
    {
        /// <summary>
        /// 匹配不支持自定义 Temperature 的模型：gpt-5*、o1*、o3*、o4*。
        /// </summary>
        private static readonly Regex TemperatureUnsupportedModelRegex = new(
            @"^(?:gpt-5|o1|o3|o4)(?:$|[.\-_])",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// 判断模型是否不支持在请求中携带 <c>temperature</c>（以及同类采样参数）。
        /// GPT-5 及以上推理模型、o1/o3/o4 系列仅允许默认值；提交非默认值或携带该字段可能导致 400。
        /// </summary>
        /// <param name="modelName">Chat / Completion 模型名（如 gpt-5.6-sol、gpt-5-mini、o3-mini）</param>
        public static bool DoesNotSupportTemperature(string? modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                return false;
            }

            var name = modelName.Trim();

            // 兼容 "provider/model" 或部署名前缀
            var slashIndex = name.LastIndexOf('/');
            if (slashIndex >= 0 && slashIndex < name.Length - 1)
            {
                name = name.Substring(slashIndex + 1);
            }

            return TemperatureUnsupportedModelRegex.IsMatch(name);
        }

        /// <summary>
        /// 判断模型是否支持自定义 Temperature。
        /// </summary>
        public static bool SupportsTemperature(string? modelName) => !DoesNotSupportTemperature(modelName);
    }
}
