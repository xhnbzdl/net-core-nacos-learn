using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nacos.V2;

namespace Nacos.Sample.Net6.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NacosController : ControllerBase
    {
        private readonly INacosConfigService _nacosConfigService;
        private readonly INacosNamingService _nacosNamingService;
        private static readonly CusConfigListen ConfigListen = new();

        public NacosController(INacosConfigService nacosConfigService,
            INacosNamingService nacosNamingService)
        {
            _nacosConfigService = nacosConfigService;
            _nacosNamingService = nacosNamingService;
        }
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="dataId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> GetSectionAsync(string dataId)
        {
            var res = await _nacosConfigService.GetConfig(dataId, "nacos_demo", 3000).ConfigureAwait(false);
            return res;
        }
        /// <summary>
        /// 获取数据库配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> GetDbConnectionString()
        {
            return await _nacosConfigService.GetConfig("DataBase_ConnectionString", "nacos_demo", 3000).ConfigureAwait(false);
        }
        /// <summary>
        /// 发布配置
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="group"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> SetConfig(string dataId,string group= "nacos_demo", string content="")
        {
            var res = await _nacosConfigService.PublishConfig(dataId, group , content).ConfigureAwait(false);
            return res;
        }
        /// <summary>
        /// 添加监听配置
        /// </summary>
        /// <param name="dataId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> AddListener(string dataId = "DataBase_ConnectionString")
        {
            await _nacosConfigService.AddListener(dataId, "nacos_demo", ConfigListen).ConfigureAwait(false);
            return "ok";
        }
        /// <summary>
        /// 移除监听
        /// </summary>
        /// <param name="dataId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> UnListener(string dataId = "DataBase_ConnectionString")
        {
            await _nacosConfigService.RemoveListener(dataId, "nacos_demo", ConfigListen).ConfigureAwait(false);

            return "ok";
        }
        /// <summary>
        /// 测试调用服务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> TestCallService()
        {
            // 服务注册时nacos会自动获取本机ip，如果项目启动url为localhost，会出现计算机拒绝访问，修改lanuchSettings.json
            // 启动地址改为本机ip即可

            // 这里需要知道被调用方的服务名
            // 获取服务实例
            var instance = await _nacosNamingService.SelectOneHealthyInstance("NacosDemoApi", "nacos_demo").ConfigureAwait(false);
            var host = $"{instance.Ip}:{instance.Port}";
            var baseUrl = instance.Metadata.TryGetValue("secure", out _) ? $"https://{host}" : $"http://{host}";

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return "empty";
            }

            var url = $"{baseUrl}/api/nacos/getDBConnectionString";

            using var client = new HttpClient();
            var result = await client.GetAsync(url);
            return await result.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// 自定义监听
        /// </summary>
        public class CusConfigListen : IListener
        {
            /// <summary>
            /// 实现接口
            /// </summary>
            /// <param name="configInfo"></param>
            public void ReceiveConfigInfo(string configInfo)
            {
                Console.WriteLine("config updating " + configInfo,ConsoleColor.Red);
            }
        }
    }
}