using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Samples.Consoles
{
    public static class SampleHelper
    {
        //public static string Default_Chat_ModeName = "gpt-35-turbo";//"chatglm2";//"gpt-35-turbo";//gpt-4-1106
        //public static string Default_TextCompletion_ModeName = "text-davinci-003";//gpt-4-1106
        //public static string Default_TextEmbedding_ModeName = "text-embedding-ada-002";
        //public static string Default_TextCompletion_ModeName = "chatglm2";
        //public static string Default_TextEmbedding_ModeName = "chatglm2";

        /// <summary>
        /// Get AppSettings file name.
        /// </summary>
        /// <returns></returns>
        public static string GetAppSettingsFile()
        {
            if (File.Exists("appsettings.Development.json"))
            {
                Console.WriteLine("use appsettings.Development.json");
                return "appsettings.Development.json";
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

            // 确保选项列表在屏幕范围内
            if (savedCursorTop + options.Length >= Console.BufferHeight)
            {
                Console.SetCursorPosition(savedCursorLeft, Console.BufferHeight - options.Length - 1);
                savedCursorTop = Console.CursorTop;
            }

            ConsoleKey key;
            PrintOptions(options, currentSelection, savedCursorTop);

            do
            {
                key = Console.ReadKey(true).Key; // 读取键盘输入

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        if (currentSelection > 0)
                        {
                            currentSelection--;
                            PrintOptions(options, currentSelection, savedCursorTop); // 更新选项显示
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (currentSelection < options.Length - 1)
                        {
                            currentSelection++;
                            PrintOptions(options, currentSelection, savedCursorTop); // 更新选项显示
                        }
                        break;
                }
            }
            while (key != ConsoleKey.Enter); // 按回车键确认选择

            // 将光标移动到列表最后一项的下一行
            Console.SetCursorPosition(savedCursorLeft, savedCursorTop + options.Length);
            Console.WriteLine();
            Console.WriteLine("您选择了：" + options[currentSelection]);
            Console.WriteLine();

            return currentSelection;
        }

        static void PrintOptions(string[] options, int currentSelection, int cursorTop)
        {
            for (int i = 0; i < options.Length; i++)
            {
                Console.SetCursorPosition(0, cursorTop + i);

                //Console.ResetColor();

                Console.BackgroundColor = SampleSetting.BackgroundColor;
                Console.ForegroundColor = SampleSetting.ForceColor;

                if (i == currentSelection)
                {
                    Console.BackgroundColor = ConsoleColor.Gray; // 高亮显示当前选择
                    Console.ForegroundColor = ConsoleColor.Blue;
                }

                Console.WriteLine("  " + options[i].PadRight(Console.WindowWidth - options[i].Length - 2));
            }
        }
    }
}