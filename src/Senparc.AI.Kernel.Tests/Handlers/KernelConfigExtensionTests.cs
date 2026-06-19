using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.KernelConfigExtensions;
using Senparc.AI.Kernel.Tests.BaseSupport;
using Senparc.AI.Tests;
using Senparc.CO2NET.Extensions;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Senparc.AI.Kernel.Handlers.Tests
{
    [TestClass()]
    public partial class KernelConfigExtensionTests : KernelTestBase
    {
        [TestMethod()]
        public async Task ConfigModel_Embedding_MemoryInforationTest()
        {
            var serviceProvider = BaseTest.serviceProvider;

            var handler = serviceProvider.GetRequiredService<IAiHandler>()
                            as SemanticAiHandler;
            var userId = "JeffreySu";

            //Test TextEmbedding
            var iWantToRun = handler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextEmbedding, userId)
                 .ConfigModel(ConfigModel.TextCompletion, userId)
                 .BuildKernel(/*b => b.WithMemoryStorage(new VolatileMemoryStore())*/);

            var dt1 = DateTime.Now;
            const string MemoryCollectionName = "aboutMe";

            var setting = Senparc.AI.Config.SenparcAiSetting;
            string textCompletionModelName = setting.ModelName.TextCompletion;
            string textCpmpetionDeployName = setting.DeploymentName ?? textCompletionModelName;
            string embeddingModelName = setting.ModelName.Embedding;
            string embeddingDeployName = setting.DeploymentName ?? embeddingModelName;//"text-embedding-ada-002";

            //New method (asynchronous, concurrent)
            iWantToRun
                .MemorySaveInformation(textCompletionModelName, MemoryCollectionName, id: "info1", text: "My name is Andrea", azureDeployName: embeddingDeployName)
                .MemorySaveInformation(textCompletionModelName, MemoryCollectionName, id: "info2", text: "I currently work as a tourist operator", azureDeployName: embeddingDeployName)
                .MemorySaveInformation(textCompletionModelName, MemoryCollectionName, id: "info3", text: "I currently live in Seattle and have been living there since 2005", azureDeployName: embeddingDeployName)
                .MemorySaveInformation(textCompletionModelName, MemoryCollectionName, id: "info4", text: "I visited France and Italy five times since 2015", azureDeployName: embeddingDeployName)
                .MemorySaveInformation(textCompletionModelName, MemoryCollectionName, id: "info5", text: "My family is from New York", azureDeployName: embeddingDeployName)
                .MemorySaveInformation(textCompletionModelName, MemoryCollectionName, id: "info6", text: "I work for Senparc", azureDeployName: embeddingDeployName)
                .MemorySaveInformation(textCompletionModelName, MemoryCollectionName, id: "info7", text: "Suzhou Senparc Network Technology Co., Ltd. was founded in 2010, mainly engaged in mobile Internet, e-commerce, software, management system development and implementation. We have in-depth research on Artificial Intelligence, big data and paperless electronic conference systems. Senparc has 5 domestic subsidiaries and 1 overseas subsidiary(in Sydney). Our products and services have been involved in government, medical, education, military, logistics, finance and many other fields. In addition to the major provinces and cities in China, Senparc's products have entered the markets of the United States, Canada, Australia, the Netherlands, Sweden and Spain.", azureDeployName: embeddingDeployName)
                .MemoryStoreExexute();

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
                var result = await iWantToRun.MemorySearchAsync(textCompletionModelName, MemoryCollectionName, q, azureDeployName: embeddingDeployName);
                var response = result.MemoryQueryResult;
                Console.Write("Q: " + q + "\r\nA: ");
                await foreach (var resultItem in response)
                {
                    Console.Write(resultItem.Metadata.Text);
                }
                Console.Write($"\r\n-- cost {(DateTime.Now - questionDt).TotalMilliseconds}ms");
                Console.WriteLine();
            }

            // test recall

            Assert.AreEqual(0, iWantToRun.Kernel.Data.Count);

            var memory = iWantToRun.SemanticKernelHelper.TryGetMemory();

            iWantToRun.ImportPluginFromObject(new TextMemoryPlugin(memory)/*, "Retrieve"*/);//TODO: simplify method

            await Console.Out.WriteLineAsync("\nFunctionsViews:");
            foreach (var item in iWantToRun.Kernel.Plugins)
            {
                await Console.Out.WriteLineAsync(item.ToJson());
            }
            // Save, Remove, Recall, Retrieve

            //No actual Function was added. Only the default 4 exist.
            Assert.AreEqual(0 + 1/* 1 Skill */, iWantToRun.Kernel.Plugins.Count);
            Assert.AreEqual(4 /* 4 default Functions */, iWantToRun.Kernel.Plugins.First().Count());

            const string skPrompt = @"
ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

Information about me, from previous conversations:
- {{$fact1}} {{recall $fact1}}
- {{$fact2}} {{recall $fact2}}
- {{$fact3}} {{recall $fact3}}
- {{$fact4}} {{recall $fact4}}
- {{$fact5}} {{recall $fact5}}
- {{$fact6}} {{recall $fact6}}
- {{$fact7}} {{recall $fact7}}

Chat:
{{$history}}
User: {{$userInput}}
ChatBot: ";


            var chatFunction = iWantToRun.CreateFunctionFromPrompt(skPrompt, maxTokens: 200, temperature: 0.8);

            //Added 1 Function, but it does not affect the Plugin count
            Assert.AreEqual(0 + 1/* 1 Skill */, iWantToRun.Kernel.Plugins.Count);
            Assert.AreEqual(4 /* 4 default Functions */, iWantToRun.Kernel.Plugins.First().Count());

            var context = iWantToRun.CreateNewArguments().arguments;

            context["fact1"] = "what is my name?";
            context["fact2"] = "where do I live?";
            context["fact3"] = "where is my family from?";
            context["fact4"] = "where have I travelled?";
            context["fact5"] = "what do I do for work?";
            context["fact6"] = "what company I work for?";
            context["fact7"] = "how many years of R&D experience does Senparc has?";
#pragma warning disable SKEXP0052 // Type is for evaluation only and may be changed or removed in a future update. Suppress this diagnostic to continue.
            context[TextMemoryPlugin.CollectionParam] = MemoryCollectionName;
            context[TextMemoryPlugin.LimitParam] = "2";
            context[TextMemoryPlugin.RelevanceParam] = "0.8";
#pragma warning restore SKEXP0052 // Type is for evaluation only and may be changed or removed in a future update. Suppress this diagnostic to continue.

            var history = "[]";
            context["history"] = history;

            var input = "Where is my company?";
            context["userInput"] = input;

            var answerResult = await chatFunction.function.InvokeAsync(chatFunction.iWantToRun.Kernel, context);
            var answer = answerResult.GetValue<string>();

            await Console.Out.WriteLineAsync(answer.ToJson());
            Assert.IsTrue(!answer.IsNullOrEmpty());
            Assert.AreEqual(answer.ToString(), answer);

            history += $"\nUser: {input}\nChatBot: {answer}\n";
            context["history"] = history;

            await Console.Out.WriteLineAsync("===== Start recall test =====");
            await Console.Out.WriteLineAsync("Question: " + input);
            await Console.Out.WriteLineAsync("Answer: " + answer.ToString());
            Console.WriteLine();

            input = "Why do you think so? Give me your logic, please.";
            context["userInput"] = input;
            var functionResult = await chatFunction.function.InvokeAsync(chatFunction.iWantToRun.Kernel, context);
            await Console.Out.WriteLineAsync("Question: " + input);
            await Console.Out.WriteLineAsync("Result(GetValue):" + functionResult.GetValue<string>());
            await Console.Out.WriteLineAsync("Answer: " + functionResult.ToJson(true));
        }

        [TestMethod()]
        public async Task ConfigModel_Embedding_MemoryReferenceTest()
        {
            var serviceProvider = BaseTest.serviceProvider;

            var handler = serviceProvider.GetRequiredService<IAiHandler>()
                            as SemanticAiHandler;
            var userId = "JeffreySu";


            //Test TextEmbedding
            var iWantToRun = handler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextEmbedding, userId)
                 .ConfigModel(ConfigModel.TextCompletion, userId)
                 .BuildKernel(/*b => b.WithMemoryStorage(new VolatileMemoryStore())*/);

            const string memoryCollectionName = "NcfGitHub";

            var setting = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SenparcAiSetting;
            var embeddingModelName = setting.ModelName.Embedding;
            var embeddingDeployName = setting.DeploymentName ?? embeddingModelName;

            var githubFiles = new Dictionary<string, string>()
            {
                ["https://github.com/NeuCharFramework/NcfDocs/blob/main/start/home/index.md"]
                    = "README: NCF introduction, source address, QQ technical exchange group",
                ["https://github.com/NeuCharFramework/NcfDocs/blob/main/start/start-develop/get-docs.md"]
                    = "Get documents, read official documents online, open official documents from the NCF site, run documentation locally with npm after downloading the source, download documentation source, run npm commands",
                ["https://github.com/NeuCharFramework/NcfDocs/blob/main/start/start-develop/run-ncf.md"]
                    = "Run NCF with Visual Studio"
            };

            var j = 0;
            var dt2 = SystemTime.Now;

            //Load
            foreach (var entry in githubFiles)
            {
                iWantToRun.MemorySaveReference(
                    modelName: embeddingModelName,
                    collection: memoryCollectionName,
                    description: entry.Value,//Used only to display records
                    text: entry.Value,//Actually used to generate embedding
                    externalId: entry.Key,
                    externalSourceName: "NeuCharFramework",
                    azureDeployName: embeddingDeployName
                );
                Console.WriteLine($"  URL {++j} saved");
            }
            iWantToRun.MemoryStoreExexute();

            var kernelMemory = await iWantToRun.SemanticKernelHelper.TryGetMemory().GetCollectionsAsync();

            Assert.AreEqual(1, kernelMemory.Count);
            Assert.AreEqual("NcfGitHub", kernelMemory.First());


            await Console.Out.WriteLineAsync($"MemorySave cost:{SystemTime.DiffTotalMS(dt2)}ms");
            await Console.Out.WriteLineAsync();

            //Question
            var dt3 = SystemTime.Now;

            var askPrompt = "I am using Visual Studio. How do I develop with it?";
            var memories = await iWantToRun.MemorySearchAsync(embeddingModelName, memoryCollectionName, askPrompt, limit: 5, minRelevanceScore: 0.77);

            var dt4 = SystemTime.Now;

            await Console.Out.WriteLineAsync("Question:" + askPrompt);
            var h = 0;
#pragma warning disable SKEXP0003 // Type is for evaluation only and may be changed or removed in a future update. Suppress this diagnostic to continue.
            await foreach (MemoryQueryResult memory in memories.MemoryQueryResult)
            {
                await Console.Out.WriteLineAsync($"Result {++h}:");
                await Console.Out.WriteLineAsync("  URL:\t\t\t" + memory.Metadata.Id?.Trim());
                await Console.Out.WriteLineAsync("  Description:\t" + memory.Metadata.Description);
                await Console.Out.WriteLineAsync("  Text:\t\t\t" + memory.Metadata.Text);
                await Console.Out.WriteLineAsync("  Relevance:\t" + memory.Relevance);
                await Console.Out.WriteLineAsync();
            }
#pragma warning restore SKEXP0003 // Type is for evaluation only and may be changed or removed in a future update. Suppress this diagnostic to continue.
            if (h == 0)
            {
                await Console.Out.WriteLineAsync("No matching result");
            }

            await Console.Out.WriteLineAsync($" -- query cost:{SystemTime.DiffTotalMS(dt4)}ms");


        }

        //[TestMethod]
        //public async Task ConfigModel_Embedding_PluginTest()
        //{
        //}
    }
}