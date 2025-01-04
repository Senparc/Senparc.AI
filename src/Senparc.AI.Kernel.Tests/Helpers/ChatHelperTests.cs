using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Kernel.Helpers;
using Senparc.AI.Kernel.Tests.BaseSupport;
using Senparc.AI.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Helpers.Tests
{
    [TestClass()]
    public class ChatHelperTests : KernelTestBase
    {
        [TestMethod()]
        public async Task TryGetImagesBase64FromContentTest()
        {
            var prompt = @"你看，这是一个图片>>>https://sdk.weixin.senparc.com/images/book-cover-front-small-3d-transparent.png，这是另外一个图：>>> https://sdk.weixin.senparc.com/images/SenparcRobot_MiniProgram.jpg 你知道吗？";

            var result = await ChatHelper.TryGetImagesBase64FromContent(BaseTest.serviceProvider, prompt);

            Assert.AreEqual(5, result.Count);

            Assert.AreEqual(ContentType.Text, result[index: 0].Type);
            Assert.AreEqual("你看，这是一个图片", result[0].TextContent);

            Assert.AreEqual(ContentType.Image, result[index: 1].Type);

            Assert.AreEqual(ContentType.Text, result[index: 2].Type);
            Assert.AreEqual("，这是另外一个图：", result[2].TextContent);

            Assert.AreEqual(ContentType.Image, result[3].Type);

            Assert.AreEqual(ContentType.Text, result[index: 4].Type);
            Assert.AreEqual(" 你知道吗？", result[4].TextContent);
        }
    }
}