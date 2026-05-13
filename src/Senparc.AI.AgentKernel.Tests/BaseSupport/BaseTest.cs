using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.AI.Interfaces;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Tests.MockEntities;
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
        private static bool _useTestAppSettings = true;


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

            var testFile = Path.Combine(Senparc.CO2NET.Utilities.ServerUtility.AppDomainAppPath, "appsettings.Development.json");
            var appsettingFileName = _useTestAppSettings && File.Exists(testFile) 
                ? "appsettings.Development.json" 
                : "appsettings.json";

            Console.WriteLine("appsettingFileName: "+ appsettingFileName);

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
        /// 调用 RegisterService.Start()
        /// </summary>
        public void RegisterServiceStart(Action<IRegisterService> registerAction, bool autoScanExtensionCacheStrategies = false)
        {
            //注册环境
            var mockEnv = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => UnitTestHelper.RootPath);

            registerService = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(mockEnv.Object, _senparcSetting)
                .UseSenparcGlobal(autoScanExtensionCacheStrategies);

            registerAction?.Invoke(registerService);


            registerService.ChangeDefaultCacheNamespace("Senparc.AI Tests");

            registerService.UseSenparcAI();

            // 如果配置了 Redis，则进行初始化（示例）
            var redisConfigurationStr = _senparcSetting.Cache_Redis_Configuration;
            var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "#{Cache_Redis_Configuration}#" /* 未配置默认占位符 */;
            if (useRedis)
            {
                /* 说明：
                 * 1. Redis 配置请在 Config.SenparcSetting.Cache_Redis_Configuration 中填写，框架会自动读取。
                 * 2. 如果需要动态修改配置，可以通过 SetConfigurationOption 方法设置 Redis 配置。
                 */
                Senparc.CO2NET.Cache.Redis.Register.SetConfigurationOption(redisConfigurationStr);
                Console.WriteLine("初始化 Redis 配置");

                // 将缓存策略切换为 Redis（键值存储方式）
                Senparc.CO2NET.Cache.Redis.Register.UseKeyValueRedisNow(); // 使用键值存储 Redis 策略
                Console.WriteLine("已设置 Redis 为 KeyValue 缓存策略");

                //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow();// 使用 HashSet 方式的 Redis

                // 如果需要，也可以通过 CacheStrategyFactory 手动注册其他缓存策略
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);
            }
        }
    }
}
