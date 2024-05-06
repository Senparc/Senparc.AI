using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using Senparc.CO2NET;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Helpers;
using static Senparc.AI.Samples.Consoles.Samples.SearchPlugin;

namespace Senparc.AI.Samples.Consoles.Samples
{
    public class FunctionSample
    {
        private readonly IAiHandler _aiHandler;
        private readonly IServiceProvider _serviceProvider;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;

        public FunctionSample(IAiHandler aiHandler, IServiceProvider serviceProvider)
        {
            this._aiHandler = aiHandler;
            this._serviceProvider = serviceProvider;
            this._semanticAiHandler.SemanticKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);//同步日志设置状态
        }

        public async Task RunAsync()
        {
            await Console.Out.WriteLineAsync(@"您已进入 FunctionSample，输入 exit 退出。");


            string url;

            while (true)
            {
            await Console.Out.WriteLineAsync("请输入需要爬取的完整网址，如 https://sdk.weixin.senparc.com");
                url = Console.ReadLine();

                //检查 URL 是否合法
                if(url.IsNullOrEmpty() || !url.StartsWith("http"))
                {
                    await Console.Out.WriteLineAsync("请输入正确的 URL 地址！");
                    continue;
                }
               
                if (url == "exit")
                {
                    break;
                }

                //完成 Kernel 基础设置
                var iWantToRun =
                     _semanticAiHandler.IWantTo()
                            .ConfigModel(ConfigModel.TextCompletion, "Jeffrey")
                            .BuildKernel();

                //定义插件
                var searchPlugin = new SearchPlugin(iWantToRun, this._serviceProvider);

                //注册插件
                var kernelPlugin = iWantToRun.ImportPluginFromObject(searchPlugin, "SearchPlugin").kernelPlugin;

                //定义需要使用的 Function（可以多个）
                KernelFunction[] functionPiple = new[] { kernelPlugin[nameof(searchPlugin.GetHtml)] };

                //设置参数（可选）
                iWantToRun.StoredAiArguments.Context["url"] = url;
                iWantToRun.StoredAiArguments.Context["method"] = "GET";

                //创建请求对象
                var request = iWantToRun.CreateRequest(true, functionPiple);

                //执行
                var functionResult = await iWantToRun.RunAsync<SearchPluginResult>(request);

                await Console.Out.WriteLineAsync("完成 HTML 抓取：");
                await Console.Out.WriteLineAsync($"URL：{functionResult.Output.Url}");
                await Console.Out.WriteLineAsync($"耗时：{functionResult.Output.CostMS}ms");
                await Console.Out.WriteLineAsync($"HTML：{functionResult.Output.HTML}");
            }

          

        }

    }

    public class SearchPlugin
    {
        private readonly IWantToRun _iWantToRun;
        private readonly IServiceProvider _serviceProvider;
        public SearchPlugin(IWantToRun iWantToRun, IServiceProvider serviceProvider)
        {
            this._iWantToRun = iWantToRun;
            this._serviceProvider = serviceProvider;
        }

        [KernelFunction("GetHtml"), Description("网页爬虫")]
        public async Task<SearchPluginResult> GetHtml(
             [Description("URL")]
            string url,
             [Description("请求头（GET/POST）")]
            string method="GET"
         )
        {
            var result = new SearchPluginResult();
            try
            {
                var htmlContent = String.Empty;
                var startTime = SystemTime.Now;
                if (method.ToUpper() == "POST")
                {
                    htmlContent = await Senparc.CO2NET.HttpUtility.RequestUtility.HttpPostAsync(_serviceProvider, url, encoding: Encoding.UTF8, postStream: null);
                }
                else
                {
                    htmlContent = await Senparc.CO2NET.HttpUtility.RequestUtility.HttpGetAsync(_serviceProvider, url, Encoding.UTF8);
                }

                result.HTML = htmlContent;
                result.CostMS = SystemTime.DiffTotalMS(startTime);
                result.Url = url;
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


       
    }

    [Serializable]
    public class SearchPluginResult
    {
        public double CostMS { get; set; }
        public string HTML { get; set; }
        public string Url { get; set; }
    }
}
