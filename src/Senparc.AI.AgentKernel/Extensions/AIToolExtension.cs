using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Senparc.AI.AgentKernel.Extensions
{
    public static class AIToolExtension
    {
        /// <summary>
        /// 根据某个对象获取到它的 AITools，要求方法上必须有 KernelFunctionAttribute 特性标记
        /// </summary>
        /// <param name="aiHandler"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static List<AIFunction> GetAITools(this IAiHandler aiHandler, object instance)
        {
            var type = instance.GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName) // 排除 get/set 属性方法
                .Where(m => m.DeclaringType == type) // 只取当前类直接定义的方法
                .Where(m => m.GetCustomAttributes().ToList().Exists(z => z is KernelFunctionAttribute))
                .Select(m => AIFunctionFactory.Create(
                    method: m,
                    target: instance,
                    name: m.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>()?.DisplayName ?? m.Name,
                    description: m.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description
                ))
                .ToList();
            return methods;
        }
    }
}
