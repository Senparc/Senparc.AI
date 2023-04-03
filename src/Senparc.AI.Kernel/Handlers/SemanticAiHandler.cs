using Microsoft.SemanticKernel;
using Senparc.AI.Helpers;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;

namespace Senparc.AI.Kernel
{
    /// <summary>
    /// SenmanticKernel ´¦ÀíÆ÷
    /// </summary>
    public class SemanticAiHandler : IAiHandler<SenparcAiResult>
    {
        private readonly SemanticKernelHelper _skHandler;
        private readonly IKernel _kernel;

        public SemanticAiHandler(SemanticKernelHelper semanticAiHandler)
        {
            _skHandler = semanticAiHandler;
            _kernel = _skHandler.GetKernel();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="request"><inheritdoc/></param>
        /// <returns></returns>
        public SenparcAiResult Run(IAiRequest request)
        {
            _skHandler.Config(request.UserId, request.ModelName, _kernel);
            var senparcAiResult = new SenparcAiResult();
            return senparcAiResult;
        }
    }

}


