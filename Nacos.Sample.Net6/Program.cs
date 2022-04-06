using Nacos.AspNetCore.V2;
using Nacos.V2.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

#region 注册服务到容器 begin
// nacos服务注册，发现配置
builder.Services.AddNacosAspNet(configuration, "NacosConfig");
//用于添加临时服务
builder.Services.AddNacosV2Naming(x =>
{
    var serverAddresses = configuration.GetSection("NacosConfig:ServerAddresses").Get<string []>().ToList();
    x.ServerAddresses = serverAddresses;
    x.EndPoint = configuration["NacosConfig:EndPoint"];
    x.Namespace = configuration["NacosConfig:Namespace"];
    x.NamingUseRpc = configuration.GetValue<bool>("NacosConfig:NamingUseRpc");
});
// nacos配置中心，方式一：
builder.Services.AddNacosV2Config(builder.Configuration, sectionName: "NacosConfig");
// nacos配置中心，方式二：
//builder.Services.AddNacosV2Config(x =>
//{
//    x.ServerAddresses = new List<string> { "http://yxchatapi.xhnbzdl.cn:8848/" };
//    x.EndPoint = "";
//    x.Namespace = "public";
//    x.UserName = "nacos";
//    x.Password = "nacos";

//    // this sample will add the filter to encrypt the config with AES.
//    x.ConfigFilterAssemblies = new List<string> { "NacosDemoApi" };

//    // http协议设置false，rpc设置true
//    x.ConfigUseRpc = false;
//});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion 注册服务到容器 end

var app = builder.Build();

#region 配置http请求的管道，中间件 begin
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
#endregion 配置http请求的管道，中间件 end