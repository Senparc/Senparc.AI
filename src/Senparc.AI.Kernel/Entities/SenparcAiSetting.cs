using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;

namespace Senparc.AI.Kernel
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public record class SenparcAiSetting : SenparcAiSettingBase<SenparcAiSetting>, ISenparcAiSetting
    {
        public SenparcAiSetting()
        {
        }
    }
}
