﻿using Microsoft.AspNetCore.Mvc;
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
    private readonly CusListener _listener;

    public NamingController(INacosNamingService nacosNamingService,
        ILogger<NamingController> logger)
    {
        _nacosNamingService = nacosNamingService;
        _listener = new(logger);
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
        // await _nacosNamingService.RegisterInstance("myService1", "127.0.0.1", 5245);
        var instance = new Nacos.V2.Naming.Dtos.Instance
        {
            Ip = "127.0.0.1",
            Ephemeral = true,
            Port = 5245,
            ServiceName = "myService1"
        };

        await _nacosNamingService.RegisterInstance("myService1","nacos_demo", instance).ConfigureAwait(false);

        return "RegisterInstance ok";
    }
    /// <summary>
    /// 撤销注册的服务实例
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> DeregisterInstance()
    {
        // await _nacosNamingService.RegisterInstance("myService1", "127.0.0.1", 5245);
        var instance = new Nacos.V2.Naming.Dtos.Instance
        {
            Ip = "127.0.0.1",
            Ephemeral = true,
            Port = 5245,
            ServiceName = "myService1"
        };

        await _nacosNamingService.DeregisterInstance("myService1","nacos_demo", instance).ConfigureAwait(false);

        return "DeregisterInstance ok";
    }
    /// <summary>
    /// 查找服务实例
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> SelectInstances()
    {
        var list = await _nacosNamingService.SelectInstances("myService1", "nacos_demo",true, false)
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
    public async Task<string> Subscribe(string serviceName= "myService1")
    {
        // 第二次订阅时会检验是否已经订阅，这里必须将AppSettings.json中的NamingUseRpc设置为true
        // 否则在源码NamingClientProxyDelegate类中的IsSubscribed（156行）将会出现空异常 grpcClientProxy = null
        await _nacosNamingService.Subscribe(serviceName, "nacos_demo", _listener).ConfigureAwait(false);
        return "Subscribe";
    }
    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> Unsubscribe(string serviceName = "myService1")
    {
        await _nacosNamingService.Unsubscribe(serviceName, "nacos_demo", _listener).ConfigureAwait(false);
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