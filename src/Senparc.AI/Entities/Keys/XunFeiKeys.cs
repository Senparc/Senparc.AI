using Senparc.AI.Entities.Keys;

namespace Senparc.AI
{
    /// <summary>
    /// XunFei (iFLYTEK) MaaS OpenAI-compatible configuration.
    /// </summary>
    public class XunFeiKeys : BaseKeys
    {
        /// <summary>
        /// XunFei MaaS API key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// XunFei MaaS OpenAI-compatible endpoint.
        /// </summary>
        public string Endpoint { get; set; } = "https://maas-api.cn-huabei-1.xf-yun.com/v1";
    }
}
