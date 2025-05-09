using System.Text;
using Microsoft.SemanticKernel;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.CO2NET.Extensions;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.Helpers;
using DefaultNamespace;
using Senparc.AI.Samples.Consoles.Samples.Plugins;

namespace Senparc.AI.Samples.Consoles.Samples
{
    public class ChatSample
    {
        private readonly IServiceProvider _serviceProvider;
        IAiHandler _aiHandler;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;

        public ChatSample(IServiceProvider serviceProvider, IAiHandler aiHandler)
        {
            this._serviceProvider = serviceProvider;
            _aiHandler = aiHandler;
            _semanticAiHandler.SemanticKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);//同步日志设置状态

        }

        public async Task RunAsync()
        {
            await Console.Out.WriteLineAsync(@"ChatSample 开始运行");
            await Console.Out.WriteLineAsync($@"[聊天设置 - 1/2] 请输入机器人系统信息（System Message），默认信息如下，如无需修改可直接输入回车。");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("------ System Message Start ------");
            await Console.Out.WriteLineAsync(Senparc.AI.DefaultSetting.DEFAULT_SYSTEM_MESSAGE);
            await Console.Out.WriteLineAsync("------  System Message End  ------");
            await Console.Out.WriteLineAsync();

            var systemMessage = Console.ReadLine();
            systemMessage = systemMessage.IsNullOrEmpty() ? Senparc.AI.DefaultSetting.DEFAULT_SYSTEM_MESSAGE : systemMessage;

            int defaultMaxHistoryCount = 5;
            int maxHistoryCount = 0;
            while (true)
            {
                await Console.Out.WriteLineAsync($"[聊天设置 - 2/2] 请输入最大保留历史对话数量，建议 5-20 之间。留空则默认保留 {defaultMaxHistoryCount} 条。");

                var maxHistoryCountString = Console.ReadLine();
                if (maxHistoryCountString.IsNullOrEmpty())
                {
                    maxHistoryCount = defaultMaxHistoryCount;
                    break;
                }
                else if (!int.TryParse(maxHistoryCountString, out maxHistoryCount) || maxHistoryCount <= 0)
                {
                    await Console.Out.WriteLineAsync("请输入正确的数字！");
                }
                else
                {
                    break;
                }
            }

            await Console.Out.WriteLineAsync($"对话历史记录数将保留 {maxHistoryCount} 条");


            await Console.Out.WriteLineAsync();

            await Console.Out.WriteLineAsync(@"配置完成，请输入对话内容。

---------------------------------
输入 [ML] 开启单次对话的多行模式
输入 [END] 完成所有多行输入
输入 save 保存对话记录
输入 exit 退出。
---------------------------------");

            await Console.Out.WriteLineAsync();

            var parameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            //await Console.Out.WriteLineAsync(localResponse);
            //var remoteResponse = await huggingFaceRemote.CompleteAsync(Input);
            // modelName: "gpt-4-32k"*/

            var setting = (SenparcAiSetting)Senparc.AI.Config.SenparcAiSetting;//也可以留空，将自动获取

            var iWantToRun = _semanticAiHandler.ChatConfig(parameter,
                                userId: "Jeffrey",
                                maxHistoryStore: maxHistoryCount,
                                chatSystemMessage: systemMessage,
                                senparcAiSetting: setting,
                                kernelBuilderAction: kh =>
                                    kh.Plugins.AddFromType<NowPlugin>()
                                              .AddFromType<SearchPlugin>()
                                    );
            //var iWantToRun = chatConfig.iWantToRun;

            var multiLineContent = new StringBuilder();
            var useMultiLine = false;
            //开始对话
            var talkingRounds = 0;
            while (true)
            {
                await Console.Out.WriteLineAsync($"[{talkingRounds + 1}] 人类：");
                var input = Console.ReadLine() ?? "";

                if (input.IsNullOrEmpty())
                {
                    Console.WriteLine("[请输入正确的内容]");
                    continue;
                }

                talkingRounds++;

                if (input.ToUpper() == "[ML]")
                {
                    await Console.Out.WriteLineAsync("识别到多行模式，请继续输入");
                    useMultiLine = true;
                }

                while (useMultiLine)
                {
                    if (input.ToUpper() == "[END]")
                    {
                        useMultiLine = false;
                        input = multiLineContent.ToString();
                    }
                    else
                    {
                        await Console.Out.WriteLineAsync("请继续输入，直到输入 [END] 停止...");
                        input = Console.ReadLine();
                        multiLineContent.Append(input);
                    }
                }

                if (input == "exit")
                {
                    break;
                }

                if (input == "save")
                {
                    //保存到文件
                    var request = iWantToRun.CreateRequest(true);

                    //历史记录
                    //初始化对话历史（可选）
                    if (request.GetStoredArguments("history", out var historyObj) && historyObj is string historyStr)
                    {
                        var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"ChatHistory-{SystemTime.NowTicks}[{talkingRounds}].txt");
                        using (var file = File.CreateText(fileName))
                        {
                            await file.WriteLineAsync("模型信息：");
                            await file.WriteLineAsync($"{SampleSetting.CurrentSettingKey} - {SampleSetting.CurrentSetting.AiPlatform}");

                            await file.WriteLineAsync($"ModelName：{SampleSetting.CurrentSetting.ModelName.Chat}");
                            await file.WriteLineAsync($"DeploymentName：{SampleSetting.CurrentSetting.DeploymentName}");
                            await file.WriteLineAsync();
                            await file.WriteLineAsync($"保存时间：{SystemTime.Now.ToString("F")}");
                            await file.WriteLineAsync($"保存对话数：{maxHistoryCount}");
                            await file.WriteLineAsync($"System Message：{systemMessage}");
                            await file.WriteLineAsync();
                            await file.WriteLineAsync("对话记录：");

                            #region 逐行处理

                            string[] lines = historyStr.Split(new[] { '\n' }, StringSplitOptions.None);
                            StringBuilder newString = new StringBuilder();
                            int humanCount = 0;

                            foreach (string line in lines)
                            {
                                if (line.StartsWith("Human:"))
                                {
                                    humanCount++;
                                    newString.AppendLine();
                                    newString.AppendLine($"[{humanCount}] 人类：" + line.Substring("Human:".Length));
                                }
                                else if (line.StartsWith("ChatBot:"))
                                {
                                    newString.AppendLine($"[{humanCount}] 机器人：" + line.Substring("ChatBot:".Length));
                                }
                                else
                                {
                                    newString.AppendLine(line);
                                }
                            }
                            await file.WriteLineAsync(newString.ToString());
                            #endregion

                            await file.WriteLineAsync();
                            await file.FlushAsync();
                        }
                        await Console.Out.WriteLineAsync($"保存完毕： {fileName}");
                    }
                    else
                    {
                        await Console.Out.WriteLineAsync($"找不到有效的对话记录，保存失败！");
                    }

                    talkingRounds--;
                    continue;
                }

                try
                {

                    var dt = SystemTime.Now;

                    await Console.Out.WriteLineAsync($"[{talkingRounds}] 机器：");

                    var useStream = true;

                    if (useStream)
                    {
                        //使用流式输出

                        var originalColor = Console.ForegroundColor;//原始颜色
                        Action<StreamingKernelContent> streamItemProceessing = async item =>
                        {
                            await Console.Out.WriteAsync(item.ToString());

                            //每个流式输出改变一次颜色
                            if (Console.ForegroundColor == originalColor)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            else
                            {
                                Console.ForegroundColor = originalColor;
                            }
                        };

                        //输出结果
                        SenparcAiResult result = await _semanticAiHandler.ChatAsync(iWantToRun, input, streamItemProceessing);

                        if (result.GetLastFunctionResultContent().IsFunctionCall)
                        {
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"\t\t >>> 本次请求执行了 function-calling：{result.GetLastFunctionResultContent().FunctionResultContent?.FunctionName} <<<");
                        }

                        //复原颜色
                        Console.ForegroundColor = originalColor;
                    }
                    else
                    {
                        //使用整体输出
                        var result = await _semanticAiHandler.ChatAsync(iWantToRun, input);
                        await Console.Out.WriteLineAsync(result.OutputString);
                    }

                    await Console.Out.WriteLineAsync();

                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync("发生错误：" + ex.ToString());
                }
                await Console.Out.WriteLineAsync();
            }
        }
    }
}