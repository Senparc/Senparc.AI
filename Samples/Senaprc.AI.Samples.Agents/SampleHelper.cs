using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Samples.Agents
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

    }
}