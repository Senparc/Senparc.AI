﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.CoreSkills;
using Microsoft.SemanticKernel.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.Tests.BaseSupport;
using Senparc.AI.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Handlers.Tests
{
    [TestClass()]
    public class KernelConfigExtensionTests : KernelTestBase
    {


        [TestMethod()]
        public async Task ConfigModel_Embedding_MemoryInforationTest()
        {
            var serviceProvider = BaseTest.serviceProvider;

            var handler = serviceProvider.GetRequiredService<IAiHandler>()
                            as SemanticAiHandler;
            var userId = "JeffreySu";

            //测试 TextEmbedding
            var iWantToRun = handler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextEmbedding, userId, "text-embedding-ada-002")
                 .ConfigModel(ConfigModel.TextCompletion, userId, "text-davinci-003")
                 .BuildKernel(b => b.WithMemoryStorage(new VolatileMemoryStore()));

            var dt1 = DateTime.Now;
            const string MemoryCollectionName = "aboutMe";

            var useNewMethod = true;
            if (!useNewMethod)
            {
                //原始方法（异步，依次进行）
                var kernel = handler.SemanticKernelHelper.GetKernel();
                await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info1", text: "My name is Andrea");
                await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info2", text: "I currently work as a tourist operator");
                await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info3", text: "I currently live in Seattle and have been living there since 2005");
                await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info4", text: "I visited France and Italy five times since 2015");
                await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info5", text: "My family is from New York");
                await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info6", text: "I'm work for Senparc");
                await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info7", text: "Suzhou Senparc Network Technology Co., Ltd. was founded in 2010, mainly engaged in mobile Internet, e-commerce, software, management system development and implementation. We have in-depth research on Artificial Intelligence, big data and paperless electronic conference systems. Senparc has 5 domestic subsidiaries and 1 overseas subsidiary(in Sydney). Our products and services have been involved in government, medical, education, military, logistics, finance and many other fields. In addition to the major provinces and cities in China, Senparc's products have entered the markets of the United States, Canada, Australia, the Netherlands, Sweden and Spain.");
            }
            else
            {
                //新方法（异步，同时进行）
                iWantToRun
                    .MemorySaveInformation(MemoryCollectionName, id: "info1", text: "My name is Andrea")
                    .MemorySaveInformation(MemoryCollectionName, id: "info2", text: "I currently work as a tourist operator")
                    .MemorySaveInformation(MemoryCollectionName, id: "info3", text: "I currently live in Seattle and have been living there since 2005")
                    .MemorySaveInformation(MemoryCollectionName, id: "info4", text: "I visited France and Italy five times since 2015")
                    .MemorySaveInformation(MemoryCollectionName, id: "info5", text: "My family is from New York")
                    .MemorySaveInformation(MemoryCollectionName, id: "info6", text: "I work for Senparc")
                    .MemorySaveInformation(MemoryCollectionName, id: "info7", text: "Suzhou Senparc Network Technology Co., Ltd. was founded in 2010, mainly engaged in mobile Internet, e-commerce, software, management system development and implementation. We have in-depth research on Artificial Intelligence, big data and paperless electronic conference systems. Senparc has 5 domestic subsidiaries and 1 overseas subsidiary(in Sydney). Our products and services have been involved in government, medical, education, military, logistics, finance and many other fields. In addition to the major provinces and cities in China, Senparc's products have entered the markets of the United States, Canada, Australia, the Netherlands, Sweden and Spain.")
                    .MemoryStoreExexute();
            }

            var dt2 = DateTime.Now;
            Console.WriteLine("kernel.Memory.SaveInformationAsync cost:" + (dt2 - dt1).TotalMilliseconds + "ms");

            var questions = new[]
            {
    "what is my name?",
    "where do I live?",
    "where is my family from?",
    "where have I travelled?",
    "what do I do for work?",
    "what company I work for?",
    "how many years of R&D experience does Senparc has?"
};

            foreach (var q in questions)
            {
                var questionDt = DateTime.Now;
                var result = await iWantToRun.MemorySearchAsync(MemoryCollectionName, q);
                var response = result.MemoryQueryResult.FirstOrDefaultAsync();
                Console.WriteLine("Q: " + q + "\r\nA:" + response.Result?.Metadata.Text + $"\r\n -- cost {(DateTime.Now - questionDt).TotalMilliseconds}ms");
                Console.WriteLine();
            }

            return;

            //            kernel.ImportSkill(new TextMemorySkill());

            //            const string skPrompt = @"
            //ChatBot can have a conversation with you about any topic.
            //It can give explicit instructions or say 'I don't know' if it does not have an answer.

            //Information about me, from previous conversations:
            //- {{$fact1}} {{recall $fact1}}
            //- {{$fact2}} {{recall $fact2}}
            //- {{$fact3}} {{recall $fact3}}
            //- {{$fact4}} {{recall $fact4}}
            //- {{$fact5}} {{recall $fact5}}
            //- {{$fact6}} {{recall $fact6}}
            //- {{$fact7}} {{recall $fact7}}

            //Chat:
            //{{$history}}
            //User: {{$userInput}}
            //ChatBot: ";

            //            var chatFunction = kernel.CreateSemanticFunction(skPrompt, maxTokens: 200, temperature: 0.8);

            //            var context = kernel.CreateNewContext();

            //            context["fact1"] = "what is my name?";
            //            context["fact2"] = "where do I live?";
            //            context["fact3"] = "where is my family from?";
            //            context["fact4"] = "where have I travelled?";
            //            context["fact5"] = "what do I do for work?";
            //            context["fact6"] = "what's my company's name?";
            //            context["fact7"] = "tell me more about this company?";

            //            context[TextMemorySkill.CollectionParam] = MemoryCollectionName;
            //            context[TextMemorySkill.RelevanceParam] = "0.8";

            //            var history = "";
            //            context["history"] = history;
            //            Func<string, Task> Chat = async (string input) =>
            //            {
            //                var dtChat1 = DateTime.Now;
            //                // Save new message in the context variables
            //                context["userInput"] = input;
            //                var dtChat2 = DateTime.Now;

            //                // Process the user message and get an answer
            //                var answer = await chatFunction.InvokeAsync(context);
            //                var dtChat3 = DateTime.Now;

            //                // Append the new interaction to the chat history
            //                history += $"\nUser: {input}\nChatBot: {answer}\n";
            //                context["history"] = history;
            //                var dtChat4 = DateTime.Now;

            //                // Show the bot response
            //                Console.WriteLine("ChatBot: " + context + $"\n[time cost] context read 1:{(dtChat2 - dtChat1).TotalMilliseconds}ms FuncionInvoke:{(dtChat3 - dtChat2).TotalMilliseconds}ms context read 2:{(dtChat4 - dtChat3).TotalMilliseconds}ms");
            //                Console.WriteLine();
            //            };


        }

        [TestMethod()]
        public async Task ConfigModel_Embedding_MemoryReferenceTest()
        {

            var serviceProvider = BaseTest.serviceProvider;

            var handler = serviceProvider.GetRequiredService<IAiHandler>()
                            as SemanticAiHandler;
            var userId = "JeffreySu";

            //测试 TextEmbedding
            var iWantToRun = handler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextEmbedding, userId, "text-embedding-ada-002")
                 .ConfigModel(ConfigModel.TextCompletion, userId, "text-davinci-003")
                 .BuildKernel(b => b.WithMemoryStorage(new VolatileMemoryStore()));

            const string memoryCollectionName = "NcfGitHub";

            var githubFiles = new Dictionary<string, string>()
            {
                ["https://github.com/NeuCharFramework/NcfDocs/blob/main/start/home/index.md"]
                    = "README: NCF 简介，源码地址，QQ 技术交流群",
                ["https://github.com/NeuCharFramework/NcfDocs/blob/main/start/start-develop/get-docs.md"]
                    = "获取文档，在线阅读官方文档，在 NCF 站点中进入官方文档，下载源码后使用 npm 本地运行，下载文档源码，运行 npm 命令",
                ["https://github.com/NeuCharFramework/NcfDocs/blob/main/start/start-develop/run-ncf.md"]
                    = "使用 Visual Studio 运行 NCF"
            };

            var j = 0;
            var dt2 = SystemTime.Now;
            //载入
            foreach (var entry in githubFiles)
            {
                iWantToRun.MemorySaveReference(
                    collection: memoryCollectionName,
                    description: entry.Value,//真正用于生成 embedding,//只用于展示记录
                    text: entry.Value,//真正用于生成 embedding
                    externalId: entry.Key,
                    externalSourceName: "GitHub"
                );
                Console.WriteLine($"  URL {++j} saved");
            }
            iWantToRun.MemoryStoreExexute();
            await Console.Out.WriteLineAsync($"MemorySave cost:{SystemTime.DiffTotalMS(dt2)}ms");
            await Console.Out.WriteLineAsync();

            //提问
            var dt3 = SystemTime.Now;

            var askPrompt = "哪里有 NCF 的介绍？";
            var memories = iWantToRun.MemorySearchAsync(memoryCollectionName, askPrompt, limit: 5, minRelevanceScore: 0.77);

            var dt4 = SystemTime.Now;

            await Console.Out.WriteLineAsync("提问：" + askPrompt);
            var h = 0;
            await foreach (MemoryQueryResult memory in memories.Result.MemoryQueryResult)
            {
                await Console.Out.WriteLineAsync($"Result {++h}:");
                await Console.Out.WriteLineAsync("  URL:\t\t\t" + memory.Metadata.Id?.Trim());
                await Console.Out.WriteLineAsync("  Description:\t" + memory.Metadata.Description);
                await Console.Out.WriteLineAsync("  Text:\t\t\t" + memory.Metadata.Text);
                await Console.Out.WriteLineAsync("  Relevance:\t" + memory.Relevance);
                await Console.Out.WriteLineAsync();
            }
            if (h == 0)
            {
                await Console.Out.WriteLineAsync("没有匹配结果");
            }
            await Console.Out.WriteLineAsync($" -- query cost:{SystemTime.DiffTotalMS(dt4)}ms");

        }
    }
}