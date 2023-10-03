using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;


namespace Senparc.AI.Kernel.Tests.Handlers
{
    /// <summary>
    /// 测试 插件
    /// </summary>
    public sealed class TestPlugin
    {
        public TestPlugin() { }


        [SKFunction, SKName("GenerateText"), System.ComponentModel.Description("创建实体类")]
        public async Task<string> GenerateText(
            [System.ComponentModel.Description("输入要求")] string input,
            SKContext sKContext
            )
        {
            var promptTemplate = @"请根据新文本要求处理文字：
# Start
1. 去掉文字收尾的空格
2. 去掉文字之间的空格
3. 将句子的首字母改成大写
# End
" +
@$"新文本要求为：{input}";

            return promptTemplate;

        }

        [SKFunction, SKName("GenerateText2"), System.ComponentModel.Description("创建实体类")]
        public async Task GenerateText2(
         [System.ComponentModel.Description("输入要求")] string input,
         SKContext sKContext
         )
        {
            var promptTemplate = @"请根据新文本要求处理文字：
# Start
1. 去掉文字收尾的空格
2. 去掉文字之间的空格
3. 将句子的首字母改成大写
# End
" +
@$"新文本要求为：{input}";

            sKContext.Variables["promptTemplate"] = promptTemplate;

            //var result = await kernel.InvokeSemanticFunctionAsync(promptTemplate, maxTokens: 2000, temperature: 0.7, topP: 0.5);
            //return result.Result;
        }



    }
}
