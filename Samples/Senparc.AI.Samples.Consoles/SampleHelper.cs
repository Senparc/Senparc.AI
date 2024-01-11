using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Samples.Consoles
{
    public static class SampleHelper
    {
        public static string Default_Chat_ModeName = "gpt-35-turbo";//gpt-4-1106
        public static string Default_TextCompletion_ModeName = "text-davinci-003";//gpt-4-1106
        public static string Default_TextEmbedding_ModeName = "text-embedding-ada-002";
        //public static string Default_TextCompletion_ModeName = "chatglm2";
        //public static string Default_TextEmbedding_ModeName = "chatglm2";

        /// <summary>
        /// Get AppSettings file name.
        /// </summary>
        /// <returns></returns>
        public static string GetAppSettingsFile()
        {
            if (File.Exists("appsettings.test.json"))
            {
                Console.WriteLine("use appsettings.test.json");
                return "appsettings.test.json";
            }

            Console.WriteLine("use appsettings.json");

            return "appsettings.json";
        }

        public static T ChooseItems<T>()
            where T : Enum
        {
            return  (T)Enum.ToObject(typeof(T), ChooseItems(Enum.GetNames(typeof(T))));
        }

        public static int ChooseItems(IEnumerable<string> options)
        {
            int currentSelection = 0; // 默认选项是第一个

            ConsoleKey key;

            do
            {
                Console.Clear();

                for (int i = 0; i < options.Count(); i++)
                {
                    if (i == currentSelection)
                        Console.BackgroundColor = ConsoleColor.Gray; // 高亮显示当前选择

                    Console.WriteLine(options.Take(i));
                    Console.ResetColor();
                }

                key = Console.ReadKey(true).Key; // 读取键盘输入

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        currentSelection--;
                        if (currentSelection < 0)
                        {
                            currentSelection = options.Count() - 1;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        currentSelection++;
                        if (currentSelection == options.Count())
                        {
                            currentSelection = 0;
                        }
                        break;
                }
            }
            while (key != ConsoleKey.Enter); // 按回车键确认选择
            return currentSelection;
        }
    }
}