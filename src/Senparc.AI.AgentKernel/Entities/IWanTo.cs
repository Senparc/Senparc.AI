using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel.Entities;
using Senparc.AI.AgentKernel.Helpers;
using Senparc.AI.AgentKernel.Kernels;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Senparc.AI.AgentKernel.Handlers
{
    public class IWantTo
    {
        /// <summary>
        /// 暂存信息
        /// </summary>
        public ConcurrentDictionary<string, object> TempStore { get; set; } = new ConcurrentDictionary<string, object>();
        public IAIKernelBuilder KernelBuilder { get; set; }
        //public KernelConfig KernelConfig { get; set; }
        public AgentKernelHelper SemanticKernelHelper { get; set; }
        public AgentAiHandler SemanticAiHandler { get; set; }

        private ISenparcAiSetting _senparcAiSetting;
        public ISenparcAiSetting SenparcAiSetting
        {
            get
            {
                _senparcAiSetting ??= Senparc.AI.Config.SenparcAiSetting;
                return _senparcAiSetting;
            }
            set => _senparcAiSetting = value;
        }

        public AiKernel Kernel => SemanticKernelHelper.GetKernel();

        public string UserId { get; set; }
        public string ModelName { get; set; }

        public IWantTo() { }

        //public IWantTo(KernelConfig kernelConfig)
        //{
        //    KernelConfig = kernelConfig;
        //}

        public IWantTo(AgentAiHandler handler, ISenparcAiSetting senparcAiSetting)
        {
            SemanticAiHandler = handler;
            SemanticKernelHelper = handler.AgentKernelHelper;
            SenparcAiSetting = senparcAiSetting ?? SemanticKernelHelper.AiSetting ?? Senparc.AI.Config.SenparcAiSetting;
        }


        //public IWantTo Config(string userId, string modelName)
        //{
        //    UserId = userId;
        //    ModelName = modelName;
        //    return this;
        //}
    }

    public class IWantToConfig
    {
        public IWantTo IWantTo { get; set; }
        public string UserId { get; set; }
        public string ModelName { get; set; }

        public AgentAiHandler SemanticAiHandler => IWantTo.SemanticAiHandler;
        public AgentKernelHelper SemanticKernelHelper => IWantTo.SemanticKernelHelper;
        public AiKernel Kernel => SemanticKernelHelper.GetKernel();

        public IWantToConfig(IWantTo iWantTo)
        {
            IWantTo = iWantTo;
        }
    }

    public class IWantToBuild
    {
        public IWantToConfig IWantToConfig { get; set; }

        public AgentAiHandler SemanticAiHandler => IWantToConfig.IWantTo.SemanticAiHandler;
        public AgentKernelHelper SemanticKernelHelper => IWantToConfig.IWantTo.SemanticKernelHelper;
        public AiKernel Kernel => SemanticKernelHelper.GetKernel();

        public IWantToBuild(IWantToConfig iWantToConfig)
        {
            IWantToConfig = iWantToConfig;
        }
    }

    public class IWantToRun
    {
        public IWantToBuild IWantToBuild { get; set; }
        //public KernelFunction KernelFunction { get; set; }
        public SenparcAiArguments StoredAiArguments { get; set; }
        public PromptConfigParameter PromptConfigParameter { get; set; }

        public List<AIFunction> Functions { get; set; }

        public AgentAiHandler SemanticAiHandler => IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
        public AgentKernelHelper SemanticKernelHelper => IWantToBuild.IWantToConfig.IWantTo.SemanticKernelHelper;
        public AiKernel Kernel => SemanticKernelHelper.GetKernel();
        public IWantToRun(IWantToBuild iWantToBuild)
        {
            IWantToBuild = iWantToBuild;
            StoredAiArguments = new SenparcAiArguments();
            Functions = new List<AIFunction>();
        }

        /// <summary>
        /// Kernel.GetService<T>(name);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        //public T GetRequiredService<T>(string? name = null)
        //    where T : class, IAIService
        //{
        //    return Kernel.GetRequiredService<T>(name);
        //}

        /// <summary>
        /// 获取当前类型的所有服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        //public IEnumerable<T> GetAllServices<T>()
        //   where T : class, IAIService
        //{
        //    return Kernel.GetAllServices<T>();
        //}
    }
}
