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
            var prompt = @"look, this is an image>>>https://sdk.weixin.senparc.com/images/book-cover-front-small-3d-transparent.png, this is another image:>>> https://sdk.weixin.senparc.com/images/SenparcRobot_MiniProgram.jpg do you know?";

            var result = await ChatHelper.TryGetImagesBase64FromContent(BaseTest.serviceProvider, prompt);

            Assert.AreEqual(5, result.Count);

            //Assert.AreEqual(ContentType.Text, result[index: 0].Type);
            //Assert.AreEqual("look, this is an image", result[0].TextContent);

            //Assert.AreEqual(ContentType.Image, result[index: 1].Type);

            //Assert.AreEqual(ContentType.Text, result[index: 2].Type);
            //Assert.AreEqual(", this is another image:", result[2].TextContent);

            //Assert.AreEqual(ContentType.Image, result[3].Type);

            //Assert.AreEqual(ContentType.Text, result[index: 4].Type);
            //Assert.AreEqual(" do you know?", result[4].TextContent);
        }
    }
}