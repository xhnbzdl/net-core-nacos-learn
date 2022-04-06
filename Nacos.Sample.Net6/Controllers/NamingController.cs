using Microsoft.AspNetCore.Mvc;
using Nacos.V2;
using Nacos.V2.Utils;

namespace Nacos.Sample.Net6.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class NamingController : ControllerBase
{
    private readonly INacosNamingService _nacosNamingService;

    public NamingController(INacosNamingService nacosNamingService)
    {
        _nacosNamingService = nacosNamingService;
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
        var list = await _nacosNamingService.GetServicesOfServer(1, 10).ConfigureAwait(false);

        var res = list.ToJsonString();

        return res ?? "GetServicesOfServer";
    }
    [HttpGet]
    public async Task<string> Subscribe(string serviceName= "myService")
    {
        await _nacosNamingService.Subscribe(serviceName, "nacos_demo", Listener).ConfigureAwait(false);
        return "Subscribe";
    }

    [HttpGet]
    public async Task<string> Unsubscribe(string serviceName = "myService")
    {
        await _nacosNamingService.Unsubscribe(serviceName, "nacos_demo", Listener).ConfigureAwait(false);
        return "UnSubscribe";
    }

    // 注意:必须保持订阅和取消订阅，以使用侦听器的一个实例!!
    // 不要为每个操作创建新的实例!!
    private static readonly CusListener Listener = new();

    public class CusListener : Nacos.V2.IEventListener
    {
        public Task OnEvent(Nacos.V2.IEvent @event)
        {
            if (@event is Nacos.V2.Naming.Event.InstancesChangeEvent e)
            {
                Console.WriteLine("CusListener", ConsoleColor.Red);
                Console.WriteLine("GroupName" + e.GroupName, ConsoleColor.Red);
                Console.WriteLine("ServiceName" + e.ServiceName, ConsoleColor.Red);
                Console.WriteLine("Clusters" + e.Clusters, ConsoleColor.Red);
                Console.WriteLine("Hosts" + e.Hosts.ToJsonString(), ConsoleColor.Red);
            }

            return Task.CompletedTask;
        }
    }
}