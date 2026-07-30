/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    文件名：OpenAIKeys.cs
    文件功能描述：定义 OpenAI 服务访问所需的密钥与可选终结点配置。


    创建标识：Senparc - 20230420

    修改标识：Senparc - 20260731
    修改描述：v0.27.4 改用 System.Text.Json 序列化特性并保留空终结点忽略行为

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Senparc.AI.Entities.Keys;

namespace Senparc.AI
{
    public class OpenAIKeys : BaseKeys
    {
        public string ApiKey { get; set; }
        public string OrganizationId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? OpenAIEndpoint { get; set; }
    }
}
