using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Entities
{
    public class ParameterConfig
    {
        public int MaxTokens { get; set; }
        public double Temperature { get; set; }
        public double TopP { get; set; }
        public double PresencePenalty { get; set; }
        public double FrequencyPenalty { get; set; }
        public List<string> StopSequences { get; set; } = new();
    }
}
