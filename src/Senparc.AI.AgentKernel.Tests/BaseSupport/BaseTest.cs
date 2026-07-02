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
        /// register IServiceCollection and MemoryCache
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

            serviceCollection.AddMemoryCache();//use memory cache

            serviceCollection.AddSenparcAI(config, _senparcAiSetting);

            Console.WriteLine("current AiPlatform: " + Senparc.AI.Config.SenparcAiSetting.AiPlatform);
            Console.WriteLine("current ModelName: " + Senparc.AI.Config.SenparcAiSetting.ModelName.Chat);


            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// Call RegisterService.Start()
        /// </summary>
        public void RegisterServiceStart(Action<IRegisterService> registerAction, bool autoScanExtensionCacheStrategies = false)
        {
            //register environment
            var mockEnv = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => UnitTestHelper.RootPath);

            registerService = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(mockEnv.Object, _senparcSetting)
                .UseSenparcGlobal(autoScanExtensionCacheStrategies);

            registerAction?.Invoke(registerService);


            registerService.ChangeDefaultCacheNamespace("Senparc.AI Tests");

            registerService.UseSenparcAI();

            // If Redis is configured, initialize it (sample).
            var redisConfigurationStr = _senparcSetting.Cache_Redis_Configuration;
            var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "#{Cache_Redis_Configuration}#" /* default placeholder is not configured */;
            if (useRedis)
            {
                /* Description:
                 * 1. Redis configure in Config.SenparcSetting.Cache_Redis_Configuration fill in, the framework reads automatically.
                 * 2. if dynamic configuration changes are required, Redis configuration can be set through SetConfigurationOption.
                 */
                Senparc.CO2NET.Cache.Redis.Register.SetConfigurationOption(redisConfigurationStr);
                Console.WriteLine("Initialize Redis configuration");

                // switch the cache strategy to Redis(key-value storage mode)
                Senparc.CO2NET.Cache.Redis.Register.UseKeyValueRedisNow(); // use the key-value Redis strategy
                Console.WriteLine("set Redis as KeyValue cache strategy");

                //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow();// use the HashSet Redis strategy

                // If needed, other cache strategies can also be registered manually through CacheStrategyFactory
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);
            }
        }
    }
}
