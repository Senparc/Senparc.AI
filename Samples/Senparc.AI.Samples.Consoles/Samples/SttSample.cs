using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Samples.Consoles.Samples
{
    public class SttSample
    {
        private readonly IServiceProvider _serviceProvider;
        IAiHandler _aiHandler;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;

        public SttSample(IServiceProvider serviceProvider, IAiHandler aiHandler)
        {
            this._serviceProvider = serviceProvider;
            _aiHandler = aiHandler;
            _semanticAiHandler.SemanticKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);//同步日志设置状态
        }

        public async Task RunAsync()
        {

        }
    }
}
