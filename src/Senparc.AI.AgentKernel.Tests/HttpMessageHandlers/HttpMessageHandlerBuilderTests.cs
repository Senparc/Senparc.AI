using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.AgentKernel.HttpMessageHandlers;
using System;

namespace Senparc.AI.AgentKernel.Tests.HttpMessageHandlers
{
    [TestClass]
    public class HttpMessageHandlerBuilderTests
    {
        [TestMethod]
        public void BuildWithoutHandlersThrowsClearException()
        {
            var builder = new HttpMessageHandlerBuilder();

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build());

            Assert.AreEqual("At least one HttpMessageHandler must be registered before calling Build().", exception.Message);
        }
    }
}
