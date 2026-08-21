using DefaultNamespace;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.Helpers;
using Senparc.AI.Samples.Consoles.Samples.Plugins;
using Senparc.CO2NET.Extensions;
using System.Text;

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
            _semanticAiHandler.SemanticKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);//Synchronize logging setting state
        }

        public async Task RunAsync()
        {
            await Console.Out.WriteLineAsync(@"ChatSample started");
            await Console.Out.WriteLineAsync($@"[Chat settings - 1/2] Enter the assistant system message (System Message). The default message is shown below. Press Enter directly if no change is needed.");
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
                await Console.Out.WriteLineAsync($"[Chat settings - 2/2] Enter the maximum number of history messages to keep. 5 to 20 is recommended. Leave empty to keep the default {defaultMaxHistoryCount} items.");

                var maxHistoryCountString = Console.ReadLine();
                if (maxHistoryCountString.IsNullOrEmpty())
                {
                    maxHistoryCount = defaultMaxHistoryCount;
                    break;
                }
                else if (!int.TryParse(maxHistoryCountString, out maxHistoryCount) || maxHistoryCount <= 0)
                {
                    await Console.Out.WriteLineAsync("Enter a valid number!");
                }
                else
                {
                    break;
                }
            }

            await Console.Out.WriteLineAsync($"Conversation history count will keep {maxHistoryCount} items");


            await Console.Out.WriteLineAsync();

            await Console.Out.WriteLineAsync(@"Configuration complete. Enter conversation content.

---------------------------------
Enter [ML] to enable multiline mode for a single conversation turn
Enter [END] to finish all multiline input
Enter save to save conversation records
Enter exit to leave.
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

            var setting = Senparc.AI.Config.SenparcAiSetting;//can also be left empty; it will be obtained automatically

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
            //Start conversation
            var talkingRounds = 0;
            while (true)
            {
                await Console.Out.WriteLineAsync($"[{talkingRounds + 1}] Human:");
                var input = Console.ReadLine() ?? "";

                if (input.IsNullOrEmpty())
                {
                    Console.WriteLine("[Enter valid content]");
                    continue;
                }

                talkingRounds++;

                if (input.ToUpper() == "[ML]")
                {
                    await Console.Out.WriteLineAsync("Multiline mode detected. Continue entering text.");
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
                        await Console.Out.WriteLineAsync("Continue entering text until [END] is entered...");
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
                    //Save to file
                    var request = iWantToRun.CreateRequest(true);

                    //History records
                    //Initialize conversation history (optional)
                    if (request.GetStoredArguments("history", out var historyObj) && historyObj is string historyStr)
                    {
                        var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"ChatHistory-{SystemTime.NowTicks}[{talkingRounds}].txt");
                        using (var file = File.CreateText(fileName))
                        {
                            await file.WriteLineAsync("Model information:");
                            await file.WriteLineAsync($"{SampleSetting.CurrentSettingKey} - {SampleSetting.CurrentSetting.AiPlatform}");

                            await file.WriteLineAsync($"ModelName:{SampleSetting.CurrentSetting.ModelName.Chat}");
                            await file.WriteLineAsync($"DeploymentName:{SampleSetting.CurrentSetting.DeploymentName}");
                            await file.WriteLineAsync();
                            await file.WriteLineAsync($"Saved at:{SystemTime.Now.ToString("F")}");
                            await file.WriteLineAsync($"Saved conversation count:{maxHistoryCount}");
                            await file.WriteLineAsync($"System Message:{systemMessage}");
                            await file.WriteLineAsync();
                            await file.WriteLineAsync("Conversation records:");

                            #region Process line by line

                            string[] lines = historyStr.Split(new[] { '\n' }, StringSplitOptions.None);
                            StringBuilder newString = new StringBuilder();
                            int humanCount = 0;

                            foreach (string line in lines)
                            {
                                if (line.StartsWith("Human:"))
                                {
                                    humanCount++;
                                    newString.AppendLine();
                                    newString.AppendLine($"[{humanCount}] Human:" + line.Substring("Human:".Length));
                                }
                                else if (line.StartsWith("ChatBot:"))
                                {
                                    newString.AppendLine($"[{humanCount}] Assistant:" + line.Substring("ChatBot:".Length));
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
                        await Console.Out.WriteLineAsync($"Saved: {fileName}");
                    }
                    else
                    {
                        await Console.Out.WriteLineAsync($"No valid conversation records were found. Save failed.");
                    }

                    talkingRounds--;
                    continue;
                }

                try
                {

                    var dt = SystemTime.Now;

                    await Console.Out.WriteLineAsync($"[{talkingRounds}] Assistant:");

                    var useStream = true;

                    if (useStream)
                    {
                        //Use streaming output

                        var originalColor = Console.ForegroundColor;//Original color
                        Action<StreamingKernelContent> streamItemProceessing = async item =>
                        {
                            await Console.Out.WriteAsync(item.ToString());

                            //Change color for each streamed output
                            if (Console.ForegroundColor == originalColor)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            else
                            {
                                Console.ForegroundColor = originalColor;
                            }
                        };

                        //Output result
                        SenparcAiResult result = await _semanticAiHandler.ChatAsync(iWantToRun, input, streamItemProceessing);

                        if (result.GetLastFunctionResultContent().IsFunctionCall)
                        {
                            Console.WriteLine();
                            Console.WriteLine();

                            SampleHelper.PrintNote($"This request executed function-calling:{result.GetLastFunctionResultContent().FunctionResultContent?.FunctionName}");
                        }

                        //Restore color
                        Console.ForegroundColor = originalColor;
                    }
                    else
                    {
                        //Use full output
                        var result = await _semanticAiHandler.ChatAsync(iWantToRun, input);
                        await Console.Out.WriteLineAsync(result.OutputString);
                    }

                    await Console.Out.WriteLineAsync();

                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync("An error occurred:" + ex.ToString());
                }
                await Console.Out.WriteLineAsync();
            }
        }
    }
}