using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Senparc.AI.AgentKernel.Helpers
{
    public enum ContentType
    {
        Text,
        Image
    }

    public interface IContentItem
    {
        ContentType Type { get; set; }
    }

    public abstract class ContentItem: IContentItem
    {
        public ContentType Type { get; set; }
    }

    public class ContentItem_Text: ContentItem
    {
        public string TextContent { get; set; }
    }

    public class ContentItem_ImageBse64 : ContentItem
    {
        public ReadOnlyMemory<byte> ImageData { get; set; }
    }

    public class ContentItem_ImageUrl : ContentItem
    {
        public ImageUrl image_url { get; set; }
    }


    public class ImageUrl
    {
        public string Url { get; set; }
    }

    public static class ChatHelper
    {
        public static async Task<ReadOnlyMemory<byte>> GetBase64Images(this IServiceProvider serviceProvider, string url)
        {
            await using MemoryStream memoryStream = new MemoryStream();
            await Senparc.CO2NET.HttpUtility.Get.DownloadAsync(serviceProvider, url, memoryStream);
            var base64 = Convert.ToBase64String(memoryStream.ToArray());
            return Convert.FromBase64String(base64);
        }

        /// <summary>
        /// Try to get image Base64 encoding from content
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="content">Supports multiline text</param>
        /// <returns></returns>
        public static async Task<List<IContentItem>> TryGetImagesBase64FromContent(this IServiceProvider serviceProvider, string content)
        {
            // Define the returned list
            List<IContentItem> result = new List<IContentItem>();

            // Define the regular expression matching pattern
            string pattern = @">>>[ ]*(?<url>http(s)?://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?)";

            // Use the regular expression to match all qualifying item content
            var matches = Regex.Matches(content, pattern);

            // Initial position
            int lastIndex = 0;

            // Iterate all matches
            foreach (Match match in matches)
            {
                // Get the current match position
                int matchIndex = match.Index;

                // Get the content before the match
                string beforeMatch = content.Substring(lastIndex, matchIndex - lastIndex);
                if (!string.IsNullOrEmpty(beforeMatch))
                {
                    result.Add(new ContentItem_Text { Type = ContentType.Text, TextContent = beforeMatch });
                }

                // Add the matched URL section
                var imageUrl = match.Result("${url}");// Extract URL
                var imageData = await serviceProvider.GetBase64Images(imageUrl);
                //result.Add(new ContentItem_ImageBse64 { Type = ContentType.Image, ImageData = new ReadOnlyMemory<byte>() });
                result.Add(new ContentItem_ImageUrl
                {
                    Type = ContentType.Image,
                    image_url = new ImageUrl() { Url = Convert.ToBase64String(imageData.ToArray()) }
                });

                // Update the last match position
                lastIndex = matchIndex + match.Length;
            }

            // Process the last section
            if (lastIndex < content.Length)
            {
                string remainingContent = content.Substring(lastIndex);
                result.Add(new ContentItem_Text { Type = ContentType.Text, TextContent = remainingContent });
            }

            // Return the result list
            return result;
        }
    }
}
