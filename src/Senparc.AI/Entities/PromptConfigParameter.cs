using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
    
namespace Senparc.AI.Entities
{
    public class PromptConfigParameter
    {
        public int? MaxTokens { get; set; }

        public double? Temperature { get; set; }

        public double? TopP { get; set; }

        public double? PresencePenalty { get; set; }

        public double? FrequencyPenalty { get; set; }

        public List<string>? StopSequences { get; set; } = new();


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
