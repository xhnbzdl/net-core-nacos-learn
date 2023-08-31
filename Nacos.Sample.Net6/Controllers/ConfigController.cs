using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nacos.V2;

namespace Nacos.Sample.Net6.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private static readonly CusConfigListen _configListen = new();

        private readonly IConfiguration _configuration;
        private readonly INacosConfigService _nacosConfigService;
        private readonly IOptions<UserInfo> _userInfo0;
        private readonly IOptionsSnapshot<UserInfo> _userInfo1;
        private readonly IOptionsMonitor<UserInfo> _userInfo2;

        public ConfigController(INacosConfigService nacosConfigService,
            IConfiguration configuration,
            IOptions<UserInfo> userInfo0,
            IOptionsSnapshot<UserInfo> userInfo1,
            IOptionsMonitor<UserInfo> userInfo2,
            ILogger<ConfigController> logger)
        {
            _nacosConfigService = nacosConfigService;
            _configuration = configuration;
            _userInfo0 = userInfo0;
            _userInfo1 = userInfo1;
            _userInfo2 = userInfo2;
            _configListen.Logger = logger;
        }
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="dataId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> GetSectionAsync(string dataId, string group = "nacos_demo")
        {
            var res = await _nacosConfigService.GetConfig(dataId, group, 3000).ConfigureAwait(false);
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
        public async Task<bool> SetConfig(string dataId, string group= "nacos_demo", string content="")
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
        public async Task<string> AddListener(string dataId = "DataBase_ConnectionString", string group = "nacos_demo")
        {
            await _nacosConfigService.AddListener(dataId, group, _configListen).ConfigureAwait(false);
            return "ok";
        }
        /// <summary>
        /// 移除监听
        /// </summary>
        /// <param name="dataId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> UnListener(string dataId = "DataBase_ConnectionString", string group = "nacos_demo")
        {
            await _nacosConfigService.RemoveListener(dataId, group, _configListen).ConfigureAwait(false);

            return "ok";
        }
        /// <summary>
        /// 通过IConfiguration获取与nacos绑定的配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public UserInfo GetBindConfig()
        {
            // 绑定了的配置，如果在GUI进行了修改，程序会同步得到修改后的配置
            return _configuration.GetSection("UserInfo").Get<UserInfo>();
        }

        /// <summary>
        /// 通过IConfiguration获取与nacos绑定的配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public object GetConfig(string dataId, string group = "nacos_demo")
        {
            return _configuration.GetSection(dataId).Value;
        }

        /// <summary>
        /// 获取通过Services.Configure<UserInfo>的配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<UserInfo> GetOptionBindConfig()
        {
            return new List<UserInfo> 
            {
                // IOptions 值不会同步更新
                _userInfo0.Value,
                // IOptionsSnapshot 同步更新
                _userInfo1.Value,
                // IOptionsMonitor 同步更新
                _userInfo2.CurrentValue
            };
        }
        /// <summary>
        /// 自定义监听
        /// </summary>
        public class CusConfigListen : IListener
        {
            public ILogger Logger { get; set; }

            /// <summary>
            /// 实现接口
            /// </summary>
            /// <param name="configInfo"></param>
            public void ReceiveConfigInfo(string configInfo)
            {
                Logger.LogWarning("config updating " + configInfo);
            }
        }
    }
}