using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI
{
    /// <summary>
    /// Senparc.AI configuration
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// current configuration
        /// </summary>
        public static ISenparcAiSetting SenparcAiSetting { get; set; }

        static Config()
        {
            ////Initialize SenparcAiSettings
            //SenparcAiSettings = new SenparcAiSetting();
        }
    }
}
