using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Entities.Keys
{
    /// <summary>
    /// 包含 DeploymentName 的接口
    /// </summary>
    public interface  IDeployment
    {
        public string DeploymentName { get; set; }
    }
}
