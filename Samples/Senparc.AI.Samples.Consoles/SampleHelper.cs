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

        /// <summary>
        /// 根据枚举值提供选项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ChooseItems<T>()
            where T : Enum
        {
            return (T)Enum.ToObject(typeof(T), ChooseItems(Enum.GetNames(typeof(T))));
        }

        /// <summary>
        /// 提供选项
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static int ChooseItems(string[] options)
        {
            int currentSelection = 0; // 默认选项是第一个

            // 保存初始光标位置
            int savedCursorTop = Console.CursorTop;
            int savedCursorLeft = Console.CursorLeft;

            // 计算选项打印的起始位置
            int optionsCursorTop = Math.Min(savedCursorTop, Console.BufferHeight - options.Length - 1);

            ConsoleKey key;
            PrintOptions(options, currentSelection, optionsCursorTop);

            do
            {
                key = Console.ReadKey(true).Key; // 读取键盘输入

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        if (currentSelection > 0)
                        {
                            currentSelection--;
                            PrintOptions(options, currentSelection, optionsCursorTop); // 更新选项显示
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (currentSelection < options.Length - 1)
                        {
                            currentSelection++;
                            PrintOptions(options, currentSelection, optionsCursorTop); // 更新选项显示
                        }
                        break;
                }
            }
            while (key != ConsoleKey.Enter); // 按回车键确认选择

            // 恢复初始光标位置
            Console.SetCursorPosition(savedCursorLeft, savedCursorTop + 1);

            return currentSelection;
        }

        static void PrintOptions(string[] options, int currentSelection, int cursorTop)
        {
            int originalCursorLeft = Console.CursorLeft;

            for (int i = 0; i < options.Length; i++)
            {
                Console.SetCursorPosition(originalCursorLeft, cursorTop + i);

                if (i == currentSelection)
                {
                    Console.BackgroundColor = ConsoleColor.Gray; // 高亮显示当前选择
                    Console.ForegroundColor = ConsoleColor.Blue;
                }

                Console.WriteLine(options[i].PadRight(Console.WindowWidth - originalCursorLeft));
                Console.ResetColor();
            }
        }


    }


}