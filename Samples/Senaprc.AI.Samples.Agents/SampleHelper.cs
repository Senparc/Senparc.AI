using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Samples.Agents
{
    public static class SampleHelper
    {
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