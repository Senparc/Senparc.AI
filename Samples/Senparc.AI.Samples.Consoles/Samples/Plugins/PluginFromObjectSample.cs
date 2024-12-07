using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.Consoles.Samples.Plugins
{
    public class PluginFromObjectSample
    {
        private readonly IAiHandler _aiHandler;
        private readonly IServiceProvider _serviceProvider;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;

        public PluginFromObjectSample(IAiHandler aiHandler, IServiceProvider serviceProvider)
        {
            this._aiHandler = aiHandler;
            this._serviceProvider = serviceProvider;
            this._semanticAiHandler.SemanticKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);//同步日志设置状态
        }

        public async Task RunAsync()
        {
            await Console.Out.WriteLineAsync(@"您已进入 PluginFromObject，输入 exit 退出。");

            var exit = false;
            while (!exit)
            {
                await Console.Out.WriteLineAsync("请输入要测试的序号：");
                await Console.Out.WriteLineAsync("[0] 退出");
                await Console.Out.WriteLineAsync("[1] 无参数 Plugin");
                await Console.Out.WriteLineAsync("[2] 带参数 Plugin");
                await Console.Out.WriteLineAsync("[3] 无参数 Plugin + 带参数 Plugin 组成管道序列");

                var select = Console.ReadLine();
                switch (select)
                {
                    case "0":
                        exit = true;
                        break;
                    case "1":
                        await RunParameterlessSampleAsync();
                        break;
                    case "2":
                        await RunParametersSampleAsync();
                        break;
                    case "3":
                        await RunPiplelineSampleAsync();
                        break;
                    default:
                        await Console.Out.WriteLineAsync("选择错误，请重新选择！");
                        continue;
                }
            }
        }

        /// <summary>
        /// 无参数 Plugin
        /// </summary>
        /// <returns></returns>
        public async Task RunParameterlessSampleAsync()
        {
            ConsoleKey? input = null;
            while (input != ConsoleKey.D0)
            {
                //完成 Kernel 基础设置
                var (iWantToRun, kernelPlugin) =
                        _semanticAiHandler.IWantTo()//初始化
                            .ConfigModel(ConfigModel.TextCompletion, "Jeffrey")//配置模型类型                                         
                            .BuildKernel()//构建 Kernel
                            .ImportPluginFromObject(new SearchPlugin(), "SearchPlugin");//注册插件

                //执行
                var functionResult = await iWantToRun.RunAsync(kernelPlugin[nameof(SearchPlugin.GetURL)]);
                //说明：此处返回类型为 string，因此可以不使用泛型 RunAsync<string>，直接使用 RunAsync() 即可

                await Console.Out.WriteLineAsync($"【外部读取】随机获取 URL：{functionResult.OutputString}");

                await Console.Out.WriteLineAsync("任意键继续获取，输入 0 退出");
                input = Console.ReadKey().Key;
            }
        }

        /// <summary>
        /// 带参数 Plugin
        /// </summary>
        /// <returns></returns>
        public async Task RunParametersSampleAsync()
        {
            while (true)
            {
                await Console.Out.WriteLineAsync("请输入需要爬取的完整网址，如 https://sdk.weixin.senparc.com。输入 exit 返回上一级");
                var url = Console.ReadLine();

                if (url == "exit")
                {
                    break;
                }

                //检查 URL 是否合法
                if (url.IsNullOrEmpty() || !url.StartsWith("http"))
                {
                    await Console.Out.WriteLineAsync("请输入正确的 URL 地址！");
                    continue;
                }

                //完成 Kernel 基础设置
                var (iWantToRun, kernelPlugin) =
                     _semanticAiHandler.IWantTo()
                            .ConfigModel(ConfigModel.TextCompletion, "Jeffrey")
                            .BuildKernel()
                            //注册插件
                            .ImportPluginFromObject(new SearchPlugin(this._serviceProvider, null), "SearchPlugin");

                //设置参数（可选）
                iWantToRun.StoredAiArguments.Context["url"] = url;
                iWantToRun.StoredAiArguments.Context["method"] = "GET";

                //创建请求对象
                var request = iWantToRun.CreateRequest(true, kernelPlugin[nameof(SearchPlugin.GetHtml)]);

                //执行
                var functionResult = await iWantToRun.RunAsync<GetHtmlResult>(request);

                await Console.Out.WriteLineAsync("==========================");
                await Console.Out.WriteLineAsync("【从 Function 外部读取】");
                await Console.Out.WriteLineAsync($"URL：{functionResult.Output.Url}");
                await Console.Out.WriteLineAsync($"耗时：{functionResult.Output.CostMS}ms");
                await Console.Out.WriteLineAsync($"HTML：{functionResult.Output.HTML}");

            }
        }

        /// <summary>
        /// 不带参数 Plugin + 带参数 Plugin 组成管道序列
        /// </summary>
        /// <returns></returns>
        public async Task RunPiplelineSampleAsync()
        {
            while (true)
            {
                await Console.Out.WriteLineAsync(@"
即将开始执行以下步骤：
1、从一个独立 Function 中随机获取一个 URL
2、自动抓取这个 URL 的内容
3、使用 AI 接口分析当前网页内容

输入回车开始，输入 exit 返回上一级
");

                var url = Console.ReadLine();

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
                var searchPlugin = new SearchPlugin(this._serviceProvider, iWantToRun,this._semanticAiHandler);

                //注册插件
                var kernelPlugin = iWantToRun.ImportPluginFromObject(searchPlugin, "SearchPlugin").kernelPlugin;

                //设置参数（可选）
                //iWantToRun.StoredAiArguments.Context["url"] = url; //URL 由 GetURL 方法返回，无需提供
                iWantToRun.StoredAiArguments.Context["method"] = "GET";

                //定义需要使用的 Function（可以多个）
                var functionPiple = new[] {
                    kernelPlugin[nameof(searchPlugin.GetURL)],
                    kernelPlugin[nameof(searchPlugin.GetHtml)],
                    kernelPlugin[nameof(searchPlugin.GetSummary)]
                };

                foreach (var function in functionPiple)
                {
                    //创建请求对象
                    var request = iWantToRun.CreateRequest(true, function);

                    //执行
                    var functionResult = await iWantToRun.RunAsync<object>(request);
                    /* 如果不需要对 request 进行额外设置，此处也可以不构建 request，直接传入 function：
                     * var functionResult = await iWantToRun.RunAsync<object>(function);
                     */
                }
            }
        }

    }

}
