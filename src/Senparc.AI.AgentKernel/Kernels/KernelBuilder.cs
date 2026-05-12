using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Kernels
{
    public interface IAIKernelBuilder
    {

    }

    public class AIKernelBuilder : IAIKernelBuilder
    {
        public IServiceCollection Services { get; set; } = new ServiceCollection();

        public IServiceProvider ServiceProvider { get; set; }

        public static AIKernelBuilder CreateBuilder()
        {
            return new AIKernelBuilder();
        }

        public void Build()
        {
            ServiceProvider = Services.BuildServiceProvider();
        }
    }
}
