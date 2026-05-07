using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Samples.Consoles.Samples.Plugins
{

    [Serializable]
    public class GetHtmlResult
    {
        public double CostMS { get; set; }
        public string HTML { get; set; }
        public string Url { get; set; }
    }
}
