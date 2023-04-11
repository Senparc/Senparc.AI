using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Interfaces
{
    public interface IAiContext
    {
        object Context { get; set; }
        //bool StoreToContainer { get; set; }
    }

    public interface IAiContext<T> : IAiContext
        where T : class
    {
        T SubContext { get; set; }
    }
}
