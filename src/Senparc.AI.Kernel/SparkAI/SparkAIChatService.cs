using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;
using Sdcb.SparkDesk;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Humanizer.On;


namespace Senparc.AI.Kernel.SparkAI
{
    /// <summary>
    /// 讯飞星火api
    /// </summary>
    public class SparkAIChatService : ITextGenerationService
    {
        private readonly SparkDeskClient _client;
        private readonly string _appId;
        private readonly string _apiKey;
        private readonly string _apiSecret;

        public SparkAIChatService(string appId, string apiKey, string apiSecret)
        {
            _appId = appId;
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _client = new SparkDeskClient(appId, apiKey, apiSecret);
        }

        public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();

        public async Task<string> ChatAsync(string[] userMessages)
        {
            StringBuilder sb = new StringBuilder();
            List<ChatMessage> messages = new List<ChatMessage>();

            foreach (var message in userMessages)
            {
                messages.Add(ChatMessage.FromUser(message));
            }

            // 假设这里的 ModelVersion.V2_0 是一个有效的版本指示。
            TokensUsage usage = await _client.ChatAsStreamAsync(ModelVersion.Max, messages.ToArray(), s => sb.Append(s), uid: "zhoujie");

            return sb.ToString();
        }

        public async IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Microsoft.SemanticKernel.Kernel? kernel = null, CancellationToken cancellationToken = default)
        {


            // 构建消息数组，这里使用 prompt 来构建 ChatMessage
            var messages = new List<ChatMessage>
    {
        ChatMessage.FromUser(prompt)
    };

            // 用于接收来自 API 的文本数据的 StringBuilder
            StringBuilder sb = new StringBuilder();

            // 调用 ChatAsStreamAsync 方法，该方法异步接收聊天消息
            //TokensUsage usage = await _client.ChatAsStreamAsync(ModelVersion.V3, messages.ToArray(), s => sb.Append(s), uid: "zhoujie", cancellationToken: cancellationToken);

            //// 解析流式数据并生成 StreamingTextContent 实例
            //// 假设每次调用返回全部文本，我们可以直接生成一个 StreamingTextContent 对象
            //yield return new StreamingTextContent(sb.ToString(), encoding: Encoding.UTF8);
            // Ensure ChatAsStreamAsync is an async streaming method
            await foreach (var response in _client.ChatAsStreamAsync(ModelVersion.Max, messages.ToArray()))
            {
                yield return new StreamingTextContent(response.Text, encoding: Encoding.UTF8);
            }
        }

        public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Microsoft.SemanticKernel.Kernel? kernel = null, CancellationToken cancellationToken = default)
        {


            // 调用 ChatAsync 方法并获取响应
            ChatResponse response = await _client.ChatAsync(ModelVersion.Max, new ChatMessage[]
            {
        ChatMessage.FromUser(prompt)
            });

            // 创建并返回 TextContent 列表
            return new List<TextContent>
    {
        new TextContent {  Text=response.Text }
    };
        }


    }

}
