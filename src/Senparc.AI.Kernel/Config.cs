﻿namespace Senparc.AI.Kernel
{
    /// <summary>
    /// Senparc.AI 配置
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// 当前配置
        /// </summary>
        public static SenparcAiSetting SenparcAiSetting
            => (SenparcAiSetting)Senparc.AI.Config.SenparcAiSetting;

        static Config()
        {
            Senparc.AI.Config.SenparcAiSetting ??= new SenparcAiSetting();
        }

    }
}
