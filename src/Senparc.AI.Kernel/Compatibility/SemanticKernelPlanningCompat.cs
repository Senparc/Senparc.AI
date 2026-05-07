using Microsoft.SemanticKernel;

namespace Microsoft.SemanticKernel.Connectors.OpenAI
{
    public class OpenAIAudioToTextExecutionSettings
    {
        public OpenAIAudioToTextExecutionSettings(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; }
        public string? Language { get; set; }
    }
}

namespace Microsoft.SemanticKernel.Memory
{
    public class MemoryRecord { }
}

namespace Microsoft.SemanticKernel.Plugins.Memory
{
    public class TextMemoryPlugin { }
}

namespace Microsoft.SemanticKernel.Planning
{
}

namespace Microsoft.SemanticKernel.Planning.Handlebars
{
    public class HandlebarsPlannerOptions
    {
        public bool AllowLoops { get; set; }
    }

    public class HandlebarsPlan
    {
        public string Prompt { get; set; } = string.Empty;

        public Task<string> InvokeAsync(Kernel kernel, KernelArguments arguments)
        {
            arguments.TryGetValue("input", out var inputObj);
            return Task.FromResult(inputObj?.ToString() ?? Prompt);
        }
    }

    public class HandlebarsPlanner
    {
        public HandlebarsPlanner(HandlebarsPlannerOptions options)
        {
        }

        public Task<HandlebarsPlan> CreatePlanAsync(Kernel kernel, string ask)
        {
            return Task.FromResult(new HandlebarsPlan { Prompt = ask });
        }
    }
}

namespace Microsoft.SemanticKernel.Text
{
    public static class TextChunker
    {
        public static List<string> SplitPlainTextLines(string text, int maxTokensPerLine) => SplitByLength(text, maxTokensPerLine);
        public static List<string> SplitPlainTextParagraphs(List<string> lines, int maxTokensPerParagraph) => MergeByLength(lines, maxTokensPerParagraph);
        public static List<string> SplitMarkDownLines(string text, int maxTokensPerLine) => SplitByLength(text, maxTokensPerLine);
        public static List<string> SplitMarkdownParagraphs(List<string> lines, int maxTokensPerParagraph) => MergeByLength(lines, maxTokensPerParagraph);

        private static List<string> SplitByLength(string text, int size)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
            {
                return result;
            }

            for (var i = 0; i < text.Length; i += size)
            {
                result.Add(text.Substring(i, Math.Min(size, text.Length - i)));
            }
            return result;
        }

        private static List<string> MergeByLength(List<string> items, int size)
        {
            var result = new List<string>();
            var buffer = string.Empty;
            foreach (var item in items)
            {
                if ((buffer.Length + item.Length) > size && buffer.Length > 0)
                {
                    result.Add(buffer);
                    buffer = item;
                }
                else
                {
                    buffer += item;
                }
            }

            if (buffer.Length > 0)
            {
                result.Add(buffer);
            }
            return result;
        }
    }
}

namespace Microsoft.SemanticKernel.TextToImage
{
    public interface ITextToImageService
    {
        Task<string> GenerateImageAsync(string imageDescription, int width, int height);
    }

    public class CompatTextToImageService : ITextToImageService
    {
        public Task<string> GenerateImageAsync(string imageDescription, int width, int height)
        {
            return Task.FromResult($"https://example.com/generated-image?prompt={Uri.EscapeDataString(imageDescription)}&w={width}&h={height}");
        }
    }
}

namespace Microsoft.SemanticKernel.AudioToText
{
    public interface IAudioToTextService
    {
        Task<TextContent> GetTextContentAsync(AudioContent audioContent, object? executionSettings = null);
    }

    public class TextContent
    {
        public string Text { get; set; } = string.Empty;
    }

    public class CompatAudioToTextService : IAudioToTextService
    {
        public Task<TextContent> GetTextContentAsync(AudioContent audioContent, object? executionSettings = null)
        {
            return Task.FromResult(new TextContent
            {
                Text = "[MAF-Compat] 语音转文本演示结果。"
            });
        }
    }
}

namespace Microsoft.SemanticKernel
{
    public class AudioContent
    {
        public BinaryData Data { get; }
        public string? MimeType { get; }

        public AudioContent(BinaryData data, string? mimeType = null)
        {
            Data = data;
            MimeType = mimeType;
        }
    }
}
