using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Tests.MockEntities;
using Senparc.CO2NET;
using Senparc.CO2NET.AspNet.RegisterServices;
using Senparc.CO2NET.RegisterServices;

namespace Senparc.AI.Tests
{
    //[TestClass]
    public class BaseTest
    {
        public static IServiceProvider serviceProvider;
        protected static IRegisterService registerService;
        protected static SenparcSetting _senparcSetting;
        protected static ISenparcAiSetting _senparcAiSetting;
        private static bool _useTestAppSettings = false;


        public BaseTest(Action<IRegisterService> registerAction, Func<IConfigurationRoot, ISenparcAiSetting> senparcAiSettingFunc,
            Action<ServiceCollection> serviceAction)
        {
            RegisterServiceCollection(senparcAiSettingFunc, serviceAction);

            RegisterServiceStart(registerAction);
        }

        public BaseTest() : this(null, null, null)
        {

        }

        /// <summary>
        /// 注册 IServiceCollection 和 MemoryCache
        /// </summary>
        public void RegisterServiceCollection(Func<IConfigurationRoot, ISenparcAiSetting> senparcAiSettingFunc, Action<ServiceCollection> serviceAction)
        {
            var serviceCollection = new ServiceCollection();

            var configBuilder = new ConfigurationBuilder();

            var testFile = Path.Combine(Senparc.CO2NET.Utilities.ServerUtility.AppDomainAppPath, "appsettings.test.json");
            var appsettingFileName = _useTestAppSettings && File.Exists(testFile) 
                ? "appsettings.test.json" 
                : "appsettings.json";

            configBuilder.AddJsonFile(appsettingFileName, false, false);
            var config = configBuilder.Build();
            serviceCollection.AddSenparcGlobalServices(config);

            _senparcSetting = new SenparcSetting() { IsDebug = true };
            config.GetSection("SenparcSetting").Bind(_senparcSetting);

            _senparcAiSetting ??= senparcAiSettingFunc?.Invoke(config)
                                   ??new SenparcAiSetting() /*new MockSenparcAiSetting() */{ IsDebug = true };

            config.GetSection("SenparcAiSetting").Bind(_senparcAiSetting);

            serviceAction?.Invoke(serviceCollection);

            serviceCollection.AddMemoryCache();//使用内存缓存

            serviceCollection.AddSenparcAI(config, _senparcAiSetting);

            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// 注册 RegisterService.Start()
        /// </summary>
        public void RegisterServiceStart(Action<IRegisterService> registerAction, bool autoScanExtensionCacheStrategies = false)
        {
            //注册
            var mockEnv = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => UnitTestHelper.RootPath);

            registerService = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(mockEnv.Object, _senparcSetting)
                .UseSenparcGlobal(autoScanExtensionCacheStrategies);

            registerAction?.Invoke(registerService);


            registerService.ChangeDefaultCacheNamespace("Senparc.AI Tests");

            registerService.UseSenparcAI();

            //配置全局使用Redis缓存（按需，独立）
            var redisConfigurationStr = _senparcSetting.Cache_Redis_Configuration;
            var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "#{Cache_Redis_Configuration}#"/*默认值，不启用*/;
            if (useRedis)//这里为了方便不同环境的开发者进行配置，做成了判断的方式，实际开发环境一般是确定的，这里的if条件可以忽略
            {
                /* 说明：
                 * 1、Redis 的连接字符串信息会从 Config.SenparcSetting.Cache_Redis_Configuration 自动获取并注册，如不需要修改，下方方法可以忽略
                /* 2、如需手动修改，可以通过下方 SetConfigurationOption 方法手动设置 Redis 链接信息（仅修改配置，不立即启用）
                 */
                Senparc.CO2NET.Cache.Redis.Register.SetConfigurationOption(redisConfigurationStr);
                Console.WriteLine("完成 Redis 设置");


                //以下会立即将全局缓存设置为 Redis
                Senparc.CO2NET.Cache.Redis.Register.UseKeyValueRedisNow();//键值对缓存策略（推荐）
                Console.WriteLine("启用 Redis UseKeyValue 策略");

                //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow();//HashSet储存格式的缓存策略

                //也可以通过以下方式自定义当前需要启用的缓存策略
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);//键值对
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);//HashSet
            }
        }
    }
}
