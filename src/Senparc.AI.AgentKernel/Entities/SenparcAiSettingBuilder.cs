using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Entities
{
    public class SenparcAiSettingBuilder
    {
        protected SenparcAiSetting SenparcAiSetting { get; set; }

        public SenparcAiSettingBuilder()
        { }


        public SenparcAiSetting Build()
        {
            return SenparcAiSetting;
        }
    }
}
