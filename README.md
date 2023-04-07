# Senaprc.AI
Senparc 全家桶的 AI 扩展包，目前主要集中于 LLM（大语言模型）的交互。


## 项目介绍

`Senparc.AI` 为所有标准接口和基础功能的基础模块

`Senparc.AI.Kernel` 为基于 Senparc.AI 标准，使用 [SemanticKernel](https://github.com/microsoft/semantic-kernel) 实现的接口调用，可以实现即插即用。

## 开发过程

### 第一步：配置账号

在 appsettings.json 中配置 OpenAI 或 Azure OpenAI 的接口信息，如：

```
  //CO2NET 设置
  "SenparcSetting": {
    "IsDebug": true,
    "DefaultCacheNamespace": "SenparcAiCache"
  },
  "SenparcAiSetting": {
    "IsDebug": true,
    "AiPlatform": "AzureOpenAI",
    "ApiKey": "YourKey",
    "OrgaizationId": "YourOrgId",
    //当设置 AiPlatform 为 AzureOpenAI 时需要设置以下参数：
    "AzureEndpoint": "https://xxx.openai.azure.com",
    "AzureOpenAIApiVersion": "2022-12-01"
  },
```

其中：`AiPlatform` 目前可选值为 `OpenAI` 或 `AzureOpenAI`，分别对应 OpenAI.com 官方接口，以及基于微软 Azure 的 Azure OpenAI 接口，系统会根据配置自动实现切换，无需在逻辑代码中进行判断。

仅当 `AiPlatform` 设置为 `OpenAI` 时，才需要设置 `OrgaizationId` 参数。

仅当 `AiPlatform` 设置为 `AzureOpenAI` 时，才需要设置 `AzureEndpoint` 和 `AzureOpenAIApiVersion` 参数。

### 第二步：开发

Senparc.AI 使用了创新的对话式编程体验，您无需了解过多不同平台、SDK 的详细用法，只需要按照自己的想法进行定义和编程，最后接收结果，以目前最火的聊天场景（Chat）为例：

```C#
// 创建 AI Handler 处理器（也可以通过工厂依赖注入）
var handler = new SemanticAiHandler();

// 定义 AI 接口调用参数和 Token 限制等
var promptParameter = new PromptConfigParameter()
{
    MaxTokens = 2000,
    Temperature = 0.7,
    TopP = 0.5,
};

// 准备运行
var userId = "JeffreySu";//区分用户
var modelName = "text-davinci-003";//默认使用模型
var iWantToRun = await handler
                    .IWantTo()
                    .ConfigModel(ConfigModel.TextCompletion, userId, modelName)
                    .BuildKernel()
                    .RegisterSemanticFunctionAsync(promptParameter);

// 输入/提问并获取结果
var prompt = "请问中国有多少人口？";
var aiRequest = iWantToRun.GetRequest(prompt);
var aiResult = await iWantToRun.RunAsync(aiRequest);

//aiResult.Output 结果：中国的人口约为13.8亿。
```

<img width="623" alt="image" src="https://user-images.githubusercontent.com/2281927/230152103-3486fbfc-2426-407c-bcb6-74d4485eaf91.png">


## TODO:
1. 实现更多模型和模式的匹配。
2. 实现全自动的工厂模块自动配置。
3. 集成到 [Senaprc.Weixin SDK](https://github.com/JeffreySu/WeiXinMPSDK)，0 逻辑代码实现 AI 能力接入（聊天场景为主）。
4. 集成到 [NeuCharFramework](https://github.com/NeuCharFramework/NCF)，0 逻辑代码实现 AI 能力接入（开发和云运营场景为主）。
