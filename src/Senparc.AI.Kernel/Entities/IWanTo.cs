using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Senparc.AI.Entities;

namespace Senparc.AI.Kernel.Handlers
{
    public class IWantTo
    {
        public KernelBuilder KernelBuilder { get; set; }
        public KernelConfig KernelConfig { get; set; }
        public SemanticKernelHelper SemanticKernelHelper { get; set; }
        public SemanticAiHandler SemanticAiHandler { get; set; }

        public IKernel Kernel => SemanticKernelHelper.GetKernel();

        public string UserId { get; set; }
        public string ModelName { get; set; }

        public IWantTo() { }

        public IWantTo(KernelConfig kernelConfig)
        {
            KernelConfig = kernelConfig;
        }

        public IWantTo(SemanticAiHandler handler)
        {
            SemanticAiHandler = handler;
            SemanticKernelHelper = handler.SemanticKernelHelper;
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


        public SemanticAiHandler SemanticAiHandler => IWantTo.SemanticAiHandler;
        public SemanticKernelHelper SemanticKernelHelper => IWantTo.SemanticKernelHelper;
        public IKernel Kernel => SemanticKernelHelper.GetKernel();


        public IWantToConfig(IWantTo iWantTo)
        {
            IWantTo = iWantTo;
        }
    }

    public class IWantToBuild
    {
        public IWantToConfig IWantToConfig { get; set; }

        public SemanticAiHandler SemanticAiHandler => IWantToConfig.IWantTo.SemanticAiHandler;
        public SemanticKernelHelper SemanticKernelHelper => IWantToConfig.IWantTo.SemanticKernelHelper;
        public IKernel Kernel => SemanticKernelHelper.GetKernel();

        public IWantToBuild(IWantToConfig iWantToConfig)
        {
            IWantToConfig = iWantToConfig;
        }
    }

    public class IWantToRun
    {
        public IWantToBuild IWantToBuild { get; set; }
        //public ISKFunction ISKFunction { get; set; }
        public SenparcAiContext AiContext { get; set; }
        public PromptConfigParameter PromptConfigParameter { get; set; }

        public SemanticAiHandler SemanticAiHandler => IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
        public SemanticKernelHelper SemanticKernelHelper => IWantToBuild.IWantToConfig.IWantTo.SemanticKernelHelper;
        public IKernel Kernel => SemanticKernelHelper.GetKernel();
        public IWantToRun(IWantToBuild iWantToBuild)
        {
            IWantToBuild = iWantToBuild;
        }
    }
}
