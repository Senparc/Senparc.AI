using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
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
        /// ����ö��ֵ�ṩѡ��
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ChooseItems<T>()
            where T : Enum
        {
            return (T)Enum.ToObject(typeof(T), ChooseItems(Enum.GetNames(typeof(T))));
        }

        /// <summary>
        /// �ṩѡ��
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static int ChooseItems(string[] options)
        {
            int currentSelection = 0; // Ĭ��ѡ���ǵ�һ��

            // �����ʼ���λ��
            int savedCursorTop = Console.CursorTop;
            int savedCursorLeft = Console.CursorLeft;

            // ȷ��ѡ���б�����Ļ��Χ��
            if (savedCursorTop + options.Length >= Console.BufferHeight)
            {
                Console.SetCursorPosition(savedCursorLeft, Console.BufferHeight - options.Length - 1);
                savedCursorTop = Console.CursorTop;
            }

            ConsoleKey key;
            PrintOptions(options, currentSelection, savedCursorTop);

            do
            {
                key = Console.ReadKey(true).Key; // ��ȡ��������

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        if (currentSelection > 0)
                        {
                            currentSelection--;
                            PrintOptions(options, currentSelection, savedCursorTop); // ����ѡ����ʾ
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (currentSelection < options.Length - 1)
                        {
                            currentSelection++;
                            PrintOptions(options, currentSelection, savedCursorTop); // ����ѡ����ʾ
                        }
                        break;
                }
            }
            while (key != ConsoleKey.Enter); // ���س���ȷ��ѡ��

            // ������ƶ����б����һ�����һ��
            Console.SetCursorPosition(savedCursorLeft, savedCursorTop + options.Length);
            Console.WriteLine();
            Console.WriteLine("��ѡ���ˣ�" + options[currentSelection]);
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
                    Console.BackgroundColor = ConsoleColor.Gray; // ������ʾ��ǰѡ��
                    Console.ForegroundColor = ConsoleColor.Blue;
                }

                Console.WriteLine("  " + options[i].PadRight(Console.WindowWidth - options[i].Length - 2));
            }
        }

        /// <summary>
        /// Print alert message
        /// </summary>
        /// <param name="message"></param>
        public static void PrintNote(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t >>> " + message);
            Console.ForegroundColor = oldColor;
        }
    }
}