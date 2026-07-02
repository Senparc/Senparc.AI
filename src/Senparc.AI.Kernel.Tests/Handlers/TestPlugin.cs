using Microsoft.SemanticKernel;


namespace Senparc.AI.Kernel.Tests.Handlers
{
    /// <summary>
    /// Test Function
    /// </summary>
    public sealed class TestFunction
    {
        public TestFunction() { }


        [KernelFunction("GenerateText"), System.ComponentModel.Description("create entity class")]
        public async Task<string> GenerateText(
            [System.ComponentModel.Description("input requirements")] string input,
            KernelArguments sKContext
            )
        {
            var promptTemplate = @"Process the text according to the new text requirements:
# Start
1. Trim leading and trailing spaces
2. Remove spaces between words
3. Capitalize the first letter of the sentence
# End
" +
@$"New text requirement is:{input}";

            return promptTemplate;

        }

        [KernelFunction("GenerateText2"), System.ComponentModel.Description("create entity class")]
        public async Task GenerateText2(
         //[System.ComponentModel.Description("input requirements")] string input,
         KernelArguments sKContext
         )
        {
            var promptTemplate = @"Process the text according to the new text requirements:
# Start
1. Trim leading and trailing spaces
2. Remove spaces between words
3. Capitalize the first letter of the sentence
# End
" +
"New text requirement is:{{$INPUT}}";

            sKContext["promptTemplate"] = promptTemplate;

            //var result = await kernel.InvokeSemanticFunctionAsync(promptTemplate, maxTokens: 2000, temperature: 0.7, topP: 0.5);
            //return result.Result;
        }



    }
}
