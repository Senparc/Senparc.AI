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
        /// Get AITools from an object. Methods must be marked with KernelFunctionAttribute.
        /// </summary>
        /// <param name="aiHandler"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static List<AIFunction> GetAITools(this IAiHandler aiHandler, object instance)
        {
            var type = instance.GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName) // Exclude get/set property methods
                .Where(m => m.DeclaringType == type) // Only take methods directly defined on the current class
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
