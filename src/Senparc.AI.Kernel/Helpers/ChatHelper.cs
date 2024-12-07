﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Helpers
{
    public enum ContentType
    {
        Text,
        Image
    }

    public class ContentItem
    {
        public ContentType Type { get; set; }
        public string TextContent { get; set; }
        public ReadOnlyMemory<byte> ImageData { get; set; }
    }

    public static class ChatHelper
    {
        public static async Task<ReadOnlyMemory<byte>> GetBase64Images(this IServiceProvider serviceProvider, string url)
        {
            await using MemoryStream memoryStream = new MemoryStream();
            await Senparc.CO2NET.HttpUtility.Get.DownloadAsync(serviceProvider, url, memoryStream);
            var base64  =Convert.ToBase64String(memoryStream.ToArray());
            return Convert.FromBase64String(base64);
        }

        /// <summary>
        /// 从内容中尝试获取图片的 Base64 编码
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="content">支持多行文本</param>
        /// <returns></returns>
        public static async Task<List<ContentItem>> TryGetImagesBase64FromContent(this IServiceProvider serviceProvider, string content)
        {
            // 定义返回的列表  
            List<ContentItem> result = new List<ContentItem>();

            // 定义正则表达式匹配模式  
            string pattern = @">>>[ ]*(?<url>http(s)?://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?)";

            // 使用正则表达式匹配所有符合条件的内容  
            var matches = Regex.Matches(content, pattern);

            // 初始位置  
            int lastIndex = 0;

            // 遍历所有匹配  
            foreach (Match match in matches)
            {
                // 获取当前匹配位置  
                int matchIndex = match.Index;

                // 获取匹配前的内容  
                string beforeMatch = content.Substring(lastIndex, matchIndex - lastIndex);
                if (!string.IsNullOrEmpty(beforeMatch))
                {
                    result.Add(new ContentItem { Type = ContentType.Text, TextContent = beforeMatch });
                }

                // 添加匹配的 URL 部分  
                var imageUrl = match.Result("${url}");// 提取 URL  
                var imageData = await serviceProvider.GetBase64Images(imageUrl);
                result.Add(new ContentItem { Type = ContentType.Image,  ImageData = imageData });

                // 更新最后匹配位置  
                lastIndex = matchIndex + match.Length;
            }

            // 处理最后一部分  
            if (lastIndex < content.Length)
            {
                string remainingContent = content.Substring(lastIndex);
                result.Add(new ContentItem { Type = ContentType.Text, TextContent = remainingContent });
            }

            // 返回结果列表  
            return result;
        }
    }
}