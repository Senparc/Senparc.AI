using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.CoreSkills;
using Microsoft.SemanticKernel.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.Tests.BaseSupport;
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
        public async Task ConfigModel_EmbeddingTest()
        {
            var handler = new SemanticAiHandler();
            var userId = "JeffreySu";

            //测试 TextEmbedding
            handler
                .IWantTo()
                .ConfigModel(ConfigModel.TextEmbedding, userId, "text-embedding-ada-002")
                .ConfigModel(ConfigModel.TextCompletion, userId, "text-davinci-003")
                .BuildKernel(b => b.WithMemoryStorage(new VolatileMemoryStore()));

            var dt1 = DateTime.Now;
            const string MemoryCollectionName = "aboutMe";
            var kernel = handler.SemanticKernelHelper.GetKernel();
            await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info1", text: "My name is Andrea");
            await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info2", text: "I currently work as a tourist operator");
            await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info3", text: "I currently live in Seattle and have been living there since 2005");
            await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info4", text: "I visited France and Italy five times since 2015");
            await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "info5", text: "My family is from New York");

            var dt2 = DateTime.Now;
            Console.WriteLine("kernel.Memory.SaveInformationAsync cost:" + (dt2 - dt1).TotalMilliseconds + "ms");

            var questions = new[]
            {
    "what is my name?",
    "where do I live?",
    "where is my family from?",
    "where have I travelled?",
    "what do I do for work?",
};

            foreach (var q in questions)
            {
                var questionDt = DateTime.Now;
                var response = await kernel.Memory.SearchAsync(MemoryCollectionName, q).FirstOrDefaultAsync();
                Console.WriteLine(q + " " + response?.Metadata.Text + $" -- cost {(DateTime.Now - questionDt).TotalMilliseconds}ms");
                Console.WriteLine();
            }

            kernel.ImportSkill(new TextMemorySkill());

            const string skPrompt = @"
ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

Information about me, from previous conversations:
- {{$fact1}} {{recall $fact1}}å
- {{$fact2}} {{recall $fact2}}
- {{$fact3}} {{recall $fact3}}
- {{$fact4}} {{recall $fact4}}
- {{$fact5}} {{recall $fact5}}

Chat:
{{$history}}
User: {{$userInput}}
ChatBot: ";

            var chatFunction = kernel.CreateSemanticFunction(skPrompt, maxTokens: 200, temperature: 0.8);

            var context = kernel.CreateNewContext();

            context["fact1"] = "what is my name?";
            context["fact2"] = "where do I live?";
            context["fact3"] = "where is my family from?";
            context["fact4"] = "where have I travelled?";
            context["fact5"] = "what do I do for work?";

            context[TextMemorySkill.CollectionParam] = MemoryCollectionName;
            context[TextMemorySkill.RelevanceParam] = "0.8";

            var history = "";
            context["history"] = history;
            Func<string, Task> Chat = async (string input) =>
            {
                var dtChat1 = DateTime.Now;
                // Save new message in the context variables
                context["userInput"] = input;
                var dtChat2 = DateTime.Now;

                // Process the user message and get an answer
                var answer = await chatFunction.InvokeAsync(context);
                var dtChat3 = DateTime.Now;

                // Append the new interaction to the chat history
                history += $"\nUser: {input}\nChatBot: {answer}\n";
                context["history"] = history;
                var dtChat4 = DateTime.Now;

                // Show the bot response
                Console.WriteLine("ChatBot: " + context + $"\n[time cost] context read 1:{(dtChat2 - dtChat1).TotalMilliseconds}ms FuncionInvoke:{(dtChat3 - dtChat2).TotalMilliseconds}ms context read 2:{(dtChat4 - dtChat3).TotalMilliseconds}ms");
                Console.WriteLine();
            };


        }
    }
}