using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Senparc.AI.Interfaces;

namespace Senparc.AI.Entities
{
    public class SenparcAiSettingItem<T> : Dictionary<string, T>
        where T : ISenparcAiSetting
    {
    }
}
