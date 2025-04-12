using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;

namespace Senparc.AI.Samples.Consoles.Samples.Plugins
{
    public class SearchPlugin
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWantToRun _iWantToRun;
        private readonly SemanticAiHandler _semanticAiHandler;

        public SearchPlugin(IServiceProvider serviceProvider = null, IWantToRun iWantToRun = null, SemanticAiHandler semanticAiHandler = null)
        {
            this._serviceProvider = serviceProvider;
            this._iWantToRun = iWantToRun;//此处传入 iWantToRun 对象，可以在 Function 中继续调用 AI 接口
            this._semanticAiHandler = semanticAiHandler;
        }

        //KernelFunction 可以使用静态方法，也可以使用实例方法

        [KernelFunction, Description("获取时间")]
        public async Task<string> GetURL(KernelArguments arguments)
        {
            string[] urls = new[] {
                "https://www.baidu.com",
                "https://sdk.weixin.senparc.com",
                "https://weixin.senparc.com/QA"
            };

            //随机获取一个URL
            var url = urls[new Random().Next(0, urls.Length)];
            //储存到上下文中
            arguments["url"] = url;

            await Console.Out.WriteLineAsync($"随机获取 URL：{url}");

            return url;
        }

        [KernelFunction, Description("网页爬虫")]
        public static async Task<GetHtmlResult> GetHtml(
            KernelArguments arguments,
             [Description("URL")]
            string url,
             [Description("请求头（GET/POST）")]
            string method="GET"
         )
        {
            var result = new GetHtmlResult();
            try
            {
                var htmlContent = String.Empty;
                var startTime = SystemTime.Now;
                if (method.ToUpper() == "POST")
                {
                    htmlContent = await Senparc.CO2NET.HttpUtility.RequestUtility.HttpPostAsync(Senparc.CO2NET.SenparcDI.GetServiceProvider(), url, encoding: Encoding.UTF8, postStream: null);
                }
                else
                {
                    htmlContent = await Senparc.CO2NET.HttpUtility.RequestUtility.HttpGetAsync(Senparc.CO2NET.SenparcDI.GetServiceProvider(), url, Encoding.UTF8);
                }

                result.HTML = htmlContent;
                result.CostMS = SystemTime.DiffTotalMS(startTime);
                result.Url = url;

                arguments["html"] = htmlContent;//将结果存入上下文，以便后续使用
            }
            catch (Exception)
            {
                throw;
            }

            await Console.Out.WriteLineAsync("完成 HTML 抓取");
            await Console.Out.WriteLineAsync("==========================");
            await Console.Out.WriteLineAsync("【从 Function 外部读取】");
            await Console.Out.WriteLineAsync($"URL： {result.Url}");
            await Console.Out.WriteLineAsync($"耗时：{result.CostMS}ms");
            await Console.Out.WriteLineAsync($"HTML：{result.HTML}");

            return result;
        }

        [KernelFunction, Description("网页内容总结")]
        public async Task<string> GetSummary(
            KernelArguments arguments,
            [Description("Html")]
            string html
            )
        {
            Console.WriteLine("正在生成 HTML 内容摘要");
            //HTML 去除所有HTML标签，仅保留文字
            var rawHtml = html.Length > 1000 ? html.Substring(0, 1000) : html;// System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "").Substring(0,300);

            arguments["html"] = rawHtml;

            //完成 Kernel 基础设置
            var (iWantToRun, newFunction) =
                 _semanticAiHandler.IWantTo()
                 .ConfigModel(ConfigModel.TextCompletion, "Jeffrey")
                 .BuildKernel()
                 .CreateFunctionFromPrompt("请从以下 HTML 代码中摘取重要信息，形成一段网页内容的总结：{{$html}}", promptConfigPara: new Entities.PromptConfigParameter()
                 {
                     MaxTokens = 5000,
                     Temperature = 0.7,
                     TopP = 0.5
                 }, functionName: "Summary");

            var request = iWantToRun.CreateRequest(true, newFunction);

            foreach (var item in arguments)
            {
                if (item.Value is string strVal)
                {
                    request.SetStoredContext(item.Key, strVal);
                }
            }

            var result = await iWantToRun.RunAsync(request);
            await Console.Out.WriteLineAsync("======================");
            await Console.Out.WriteLineAsync("内容总结：" + result.Output);
            return result.Output;

        }
    }

}
