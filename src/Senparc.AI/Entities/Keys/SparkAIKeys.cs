using System;
using System.Collections.Generic;
using System.Text;
using Senparc.AI.Entities.Keys;

namespace Senparc.AI
{
    /// <summary>
    /// 科大讯飞
    /// </summary>
    public class SparkAIKeys : BaseKeys
    {
        //public string ApiKey { get; set; }
        //public string OrganizationId { get; set; }

        // 应用APPID（必须为webapi类型应用，并开通星火认知大模型授权）
        public string AppId { get; set; }

        // 接口密钥（webapi类型应用开通星火认知大模型后，控制台--我的应用---星火认知大模型---相应服务的apikey）
        public string ApiSecret { get; set; }
        // 接口密钥（webapi类型应用开通星火认知大模型后，控制台--我的应用---星火认知大模型---相应服务的apisecret）
        public string ApiKey { get; set; }

        public string SparkAIEndpoint { get; set; }
        /// <summary>
        /// 版本 
        /// </summary>
        public string ModelVersion { get; set; }


    }
}
