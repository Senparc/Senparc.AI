namespace Senparc.AI.Kernel
{
    /// <summary>
    /// Senparc.AI configuration
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// current configuration
        /// </summary>
        public static SenparcAiSetting SenparcAiSetting
            => (SenparcAiSetting)Senparc.AI.Config.SenparcAiSetting;

        static Config()
        {
            Senparc.AI.Config.SenparcAiSetting ??= new SenparcAiSetting();
        }

    }
}
