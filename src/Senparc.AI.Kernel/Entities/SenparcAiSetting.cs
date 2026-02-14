using Senparc.AI.Entities;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
