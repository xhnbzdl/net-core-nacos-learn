using Microsoft.AspNetCore.Mvc;
using Nacos.V2;
using Nacos.V2.Utils;

namespace Nacos.Sample.Net6.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class NamingController : ControllerBase
{
    private readonly INacosNamingService _nacosNamingService;
    // 注意:必须保持订阅和取消订阅，以使用侦听器的一个实例!!
    // 不要为每个操作创建新的实例!!
    private readonly CusListener Listener;

    public NamingController(INacosNamingService nacosNamingService,
        ILogger<NamingController> logger)
    {
        _nacosNamingService = nacosNamingService;
        Listener = new(logger);
    }
    /// <summary>
    /// 获取所有服务实例
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> GetAllInstances()
    {
        // groupName default = DEFAULT_GROUP
        var list = await _nacosNamingService.GetAllInstances("NacosDemoApi", "nacos_demo", false).ConfigureAwait(false);

        var res = list.ToJsonString();

        return res ?? "GetAllInstances";
    }
    /// <summary>
    /// 注册服务实例
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> RegisterInstance()
    {
        // await _nacosNamingService.RegisterInstance("myService", "127.0.0.1", 5245);
        var instance = new Nacos.V2.Naming.Dtos.Instance
        {
            Ip = "127.0.0.1",
            Ephemeral = true,
            Port = 5245,
            ServiceName = "myService"
        };

        await _nacosNamingService.RegisterInstance("myService","nacos_demo", instance).ConfigureAwait(false);

        return "RegisterInstance ok";
    }
    /// <summary>
    /// 撤销注册的服务实例
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> DeregisterInstance()
    {
        // await _nacosNamingService.RegisterInstance("myService", "127.0.0.1", 5245);
        var instance = new Nacos.V2.Naming.Dtos.Instance
        {
            Ip = "127.0.0.1",
            Ephemeral = true,
            Port = 5245,
            ServiceName = "myService"
        };

        await _nacosNamingService.DeregisterInstance("myService","nacos_demo", instance).ConfigureAwait(false);

        return "DeregisterInstance ok";
    }
    /// <summary>
    /// 查找服务实例
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> SelectInstances()
    {
        var list = await _nacosNamingService.SelectInstances("myService", "nacos_demo",true, false)
            .ConfigureAwait(false);

        var res = list.ToJsonString();

        return res ?? "SelectInstances ok";
    }
    [HttpGet]
    public async Task<string> GetServicesOfServer()
    {
        var list = await _nacosNamingService.GetServicesOfServer(1, 10,"nacos_demo").ConfigureAwait(false);

        var res = list.ToJsonString();

        return res ?? "GetServicesOfServer";
    }
    /// <summary>
    /// 订阅服务
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> Subscribe(string serviceName= "myService")
    {
        await _nacosNamingService.Subscribe(serviceName, "nacos_demo", Listener).ConfigureAwait(false);
        return "Subscribe";
    }
    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> Unsubscribe(string serviceName = "myService")
    {
        await _nacosNamingService.Unsubscribe(serviceName, "nacos_demo", Listener).ConfigureAwait(false);
        return "UnSubscribe";
    }
    /// <summary>
    /// 服务监听
    /// </summary>
    public class CusListener : Nacos.V2.IEventListener
    {
        private readonly ILogger _logger;

        public CusListener(ILogger logger)
        {
            _logger = logger;
        }

        public Task OnEvent(Nacos.V2.IEvent @event)
        {
            if (@event is Nacos.V2.Naming.Event.InstancesChangeEvent e)
            {
                _logger.LogWarning("==============================");
                _logger.LogWarning("CusListener");
                _logger.LogWarning("GroupName :" + e.GroupName);
                _logger.LogWarning("ServiceName :" + e.ServiceName);
                _logger.LogWarning("Clusters :" + e.Clusters);
                _logger.LogWarning("Hosts :" + e.Hosts.ToJsonString());
            }

            return Task.CompletedTask;
        }
    }
}