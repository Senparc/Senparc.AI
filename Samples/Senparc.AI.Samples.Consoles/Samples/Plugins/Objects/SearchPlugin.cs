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
            this._iWantToRun = iWantToRun;//Pass the iWantToRun object here so the AI API can continue to be called from the Function
            this._semanticAiHandler = semanticAiHandler;
        }

        //KernelFunction can use a static method or an instance method

        [KernelFunction, Description("Get URL")]
        public async Task<string> GetURL(KernelArguments arguments)
        {
            string[] urls = new[] {
                "https://www.baidu.com",
                "https://sdk.weixin.senparc.com",
                "https://weixin.senparc.com/QA"
            };

            //Randomly get a URL
            var url = urls[new Random().Next(0, urls.Length)];
            //Store in context
            arguments["url"] = url;

            await Console.Out.WriteLineAsync($"Randomly get URL:{url}");

            return url;
        }

        [KernelFunction, Description("Web crawler")]
        public static async Task<GetHtmlResult> GetHtml(
            KernelArguments arguments,
             [Description("URL")]
            string url,
             [Description("Request header (GET/POST)")]
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

                arguments["html"] = htmlContent;//Store the result in context for later use
            }
            catch (Exception)
            {
                throw;
            }

            await Console.Out.WriteLineAsync("HTML fetch completed");
            await Console.Out.WriteLineAsync("==========================");
            await Console.Out.WriteLineAsync("[Read from outside the Function]");
            await Console.Out.WriteLineAsync($"URL: {result.Url}");
            await Console.Out.WriteLineAsync($"Elapsed time:{result.CostMS}ms");
            await Console.Out.WriteLineAsync($"HTML:{result.HTML}");

            return result;
        }

        [KernelFunction, Description("Web content summary")]
        public async Task<string> GetSummary(
            KernelArguments arguments,
            [Description("Html")]
            string html
            )
        {
            Console.WriteLine("Generating HTML content summary");
            //Remove all HTML tags and keep only text
            var rawHtml = html.Length > 1000 ? html.Substring(0, 1000) : html;// System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "").Substring(0,300);

            arguments["html"] = rawHtml;

            //Kernel base setup completed
            var (iWantToRun, newFunction) =
                 _semanticAiHandler.IWantTo()
                 .ConfigModel(ConfigModel.TextCompletion, "Jeffrey")
                 .BuildKernel()
                 .CreateFunctionFromPrompt("Extract important information from the following HTML and create a web content summary: {{$html}}", promptConfigPara: new Entities.PromptConfigParameter()
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
            await Console.Out.WriteLineAsync("Content summary:" + result.Output);
            return result.Output;

        }
    }

}
