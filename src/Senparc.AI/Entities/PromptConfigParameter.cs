using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
    
namespace Senparc.AI.Entities
{
    public class PromptConfigParameter
    {
        private int? _maxTokens;
        private double? _temperature;
        private double? _topP;
        private double? _presencePenalty;
        private double? _frequencyPenalty;
        private List<string>? _stopSequences;

        public int? MaxTokens
        {
            get { return _maxTokens; }
            set { _maxTokens = value; }
        }

        public double? Temperature
        {
            get { return _temperature; }
            set { _temperature = value; }
        }

        public double? TopP
        {
            get { return _topP; }
            set { _topP = value; }
        }

        public double? PresencePenalty
        {
            get { return _presencePenalty; }
            set { _presencePenalty = value; }
        }

        public double? FrequencyPenalty
        {
            get { return _frequencyPenalty; }
            set { _frequencyPenalty = value; }
        }

        public List<string>? StopSequences
        {
            get { return _stopSequences; }
            set { _stopSequences = value; }
        }

        public PromptConfigParameter()
        {
            _stopSequences = new List<string>();
        }


        //public void TrySet(Expression<Func<PromptConfigParameter, dynamic>> condition, object targetObject, Expression<Func<object>> targetProperty)
        //{
        //    var thisPropValue = condition.Compile().Invoke(this);
        //    if (thisPropValue != null)
        //    {
        //        var member = (MemberExpression)targetProperty.Body;
        //        var prop = (PropertyInfo)member.Member;
        //        prop.SetValue(targetObject, thisPropValue);
        //    }
        //}
    }
}
