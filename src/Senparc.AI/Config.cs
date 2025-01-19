using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI
{
    /// <summary>
    /// Senparc.AI 配置
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// 当前配置
        /// </summary>
        public static ISenparcAiSetting SenparcAiSetting { get; set; }

        static Config()
        {
            ////初始化 SenparcAiSettings
            //SenparcAiSettings = new SenparcAiSetting();
        }
    }
}
