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
            this._semanticAiHandler.SemanticKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);//Synchronize logging setting state
        }

        public async Task RunAsync()
        {
            await Console.Out.WriteLineAsync(@"You have entered PluginFromObject. Enter exit to leave.");

            var exit = false;
            while (!exit)
            {
                await Console.Out.WriteLineAsync("Enter the number to test:");
                await Console.Out.WriteLineAsync("[0] Exit");
                await Console.Out.WriteLineAsync("[1] Plugin without parameters");
                await Console.Out.WriteLineAsync("[2] Plugin with parameters");
                await Console.Out.WriteLineAsync("[3] Plugin without parameters + Plugin with parameters as a pipeline sequence");

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
                        await Console.Out.WriteLineAsync("Invalid selection. Select again!");
                        continue;
                }
            }
        }

        /// <summary>
        /// Plugin without parameters
        /// </summary>
        /// <returns></returns>
        public async Task RunParameterlessSampleAsync()
        {
            ConsoleKey? input = null;
            while (input != ConsoleKey.D0)
            {
                //Kernel base setup completed
                var (iWantToRun, kernelPlugin) =
                        _semanticAiHandler.IWantTo()//Initialize
                            .ConfigModel(ConfigModel.TextCompletion, "Jeffrey")//Configure model type
                            .BuildKernel()//Build Kernel
                            .ImportPluginFromObject(new SearchPlugin(), "SearchPlugin");//Register plugin

                //Execute
                var functionResult = await iWantToRun.RunAsync(kernelPlugin[nameof(SearchPlugin.GetURL)]);
                //Description: the return type here is string, so RunAsync<string> is not required. RunAsync() can be used directly.

                await Console.Out.WriteLineAsync($"[External read]Randomly get URL:{functionResult.OutputString}");

                await Console.Out.WriteLineAsync("Press any key to continue fetching, or enter 0 to exit");
                input = Console.ReadKey().Key;
            }
        }

        /// <summary>
        /// Plugin with parameters
        /// </summary>
        /// <returns></returns>
        public async Task RunParametersSampleAsync()
        {
            while (true)
            {
                await Console.Out.WriteLineAsync("Enter the full URL to crawl, such as https://sdk.weixin.senparc.com. Enter exit to return to the previous level.");
                var url = Console.ReadLine();

                if (url == "exit")
                {
                    break;
                }

                //Check whether the URL is valid
                if (url.IsNullOrEmpty() || !url.StartsWith("http"))
                {
                    await Console.Out.WriteLineAsync("Enter a valid URL!");
                    continue;
                }

                //Kernel base setup completed
                var (iWantToRun, kernelPlugin) =
                     _semanticAiHandler.IWantTo()
                            .ConfigModel(ConfigModel.TextCompletion, "Jeffrey")
                            .BuildKernel()
                            //Register plugin
                            .ImportPluginFromObject(new SearchPlugin(this._serviceProvider, null), "SearchPlugin");

                //Set parameters (optional)
                iWantToRun.StoredAiArguments.Context["url"] = url;
                iWantToRun.StoredAiArguments.Context["method"] = "GET";

                //Create request object
                var request = iWantToRun.CreateRequest(true, kernelPlugin[nameof(SearchPlugin.GetHtml)]);

                //Execute
                var functionResult = await iWantToRun.RunAsync<GetHtmlResult>(request);

                await Console.Out.WriteLineAsync("==========================");
                await Console.Out.WriteLineAsync("[Read from outside the Function]");
                await Console.Out.WriteLineAsync($"URL:{functionResult.Output.Url}");
                await Console.Out.WriteLineAsync($"Elapsed time:{functionResult.Output.CostMS}ms");
                await Console.Out.WriteLineAsync($"HTML:{functionResult.Output.HTML}");

            }
        }

        /// <summary>
        /// notPlugin with parameters + Plugin with parameters compose a pipeline sequence
        /// </summary>
        /// <returns></returns>
        public async Task RunPiplelineSampleAsync()
        {
            while (true)
            {
                await Console.Out.WriteLineAsync(@"
About to execute the following steps:
1. Randomly get a URL from an independent Function
2. Automatically fetch the content from this URL
3. Use the AI API to analyze the current web page content

Press Enter to start. Enter exit to return to the previous level.
");

                var url = Console.ReadLine();

                if (url == "exit")
                {
                    break;
                }

                //Kernel base setup completed
                var iWantToRun =
                     _semanticAiHandler.IWantTo()
                            .ConfigModel(ConfigModel.TextCompletion, "Jeffrey")
                            .BuildKernel();

                //Define plugin
                var searchPlugin = new SearchPlugin(this._serviceProvider, iWantToRun,this._semanticAiHandler);

                //Register plugin
                var kernelPlugin = iWantToRun.ImportPluginFromObject(searchPlugin, "SearchPlugin").kernelPlugin;

                //Set parameters (optional)
                //iWantToRun.StoredAiArguments.Context["url"] = url; //URL is returned by the GetURL method and does not need to be provided
                iWantToRun.StoredAiArguments.Context["method"] = "GET";

                //Define the Functions to use (one or more)
                var functionPiple = new[] {
                    kernelPlugin[nameof(searchPlugin.GetURL)],
                    kernelPlugin[nameof(searchPlugin.GetHtml)],
                    kernelPlugin[nameof(searchPlugin.GetSummary)]
                };

                foreach (var function in functionPiple)
                {
                    //Create request object
                    var request = iWantToRun.CreateRequest(true, function);

                    //Execute
                    var functionResult = await iWantToRun.RunAsync<object>(request);
                    /* If no extra request settings are required, you can skip building the request here and pass the function directly:
                     * var functionResult = await iWantToRun.RunAsync<object>(function);
                     */
                }
            }
        }

    }

}
