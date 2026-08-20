/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    File: OpenAIKeys.cs
    Description: Defines the keys and optional endpoint configuration required to access the OpenAI service.


    Created by: Senparc - 20230420

    Modified by: Senparc - 20260731
    Change description: v0.27.4 switched to System.Text.Json serialization attributes while preserving omission of an empty endpoint.

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
