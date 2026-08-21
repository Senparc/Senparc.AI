# Command-Line Sample Usage

## 1. Open The Solution

Open `Senparc.AI.sln`, set platform parameters such as `ApiKey` in `appsettings.json`, and start the `Senparc.AI.Samples.Consoles` project.

## 2. Operations

### 2.1 Chat

Enter `1` to start the chat sample.

### 2.2 Embedding

Enter `2` from the main menu to start the Embedding sample. Embedding supports standard information and reference information modes.

### 2.2.1 Standard Embedding

Select `1` to enter the standard Embedding test. Input fields are separated by three English colons. After input is complete, enter `n` to start the conversation test.

### 2.2.2 Reference Embedding

Select `2` to enter the reference Embedding test. Input fields are separated by three English colons. After input is complete, enter `n` to start the conversation test.

### 2.3 DALL-E Image Generation

Enter `3` from the initial screen to start the DALL-E drawing operation. The result is returned as a URL. Enter `s` to save the image locally.

> Note: the URL returned by the API is temporary and cannot be used for persistent display. Save it promptly.

### 2.4 Planner

The first step selects an existing plugin provider, such as `SummarizePlugin` or `WriterPlugin`. The second step provides the plan objective directly.

## Embedding Test Data

### Standard Embedding Test

```text
My name is Jeffrey
My country is China
I'm working for Senparc
Senparc is a company based in Suzhou
Senparc is founded at 2010
Senparc startup at 2010, in Jiangsu Province, Suzhou City, Gusu Distinct
Senparc.Weixin SDK is made by Senparc, main author is Jeffrey Su who is also the founder of Senparc.
Jeffrey Su's Chinese name is Zhenwei Su.
Zhenwei Su has written two books.
```

### Reference Embedding Test

```text
https://github.com/NeuCharFramework/NcfDocs/blob/main/start/home/index.md:::README: NCF introduction, source address, QQ technical exchange group
https://github.com/NeuCharFramework/NcfDocs/blob/main/start/start-develop/get-docs.md:::Get documents, read official documents online, open official documents from the NCF site, run documentation locally with npm after downloading the source, download documentation source, run npm commands
https://github.com/NeuCharFramework/NcfDocs/blob/main/start/start-develop/run-ncf.md:::Run NCF with Visual Studio
```
