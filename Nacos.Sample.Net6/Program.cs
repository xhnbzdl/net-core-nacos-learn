using Nacos.AspNetCore.V2;
using Nacos.Sample.Net6;
using Nacos.V2.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Host.ConfigureAppConfiguration(builder =>
{
    // NuGet Package: nacos-sdk-csharp.Extensions.Configuration
    // ������չ�Դ���Configuration
    builder.AddNacosV2Configuration(configuration.GetSection("NacosConfig"));
});


#region ע��������� begin
// nacos�������
// ��������ʱ���Զ�ע�ᵱǰ������� ��ʽ1:
builder.Services.AddNacosAspNet(configuration, "NacosConfig");
// ���������ʱ���� ��ʽ2:
//builder.Services.AddNacosV2Naming(x =>
//{
//    var serverAddresses = configuration.GetSection("NacosConfig:ServerAddresses").Get<string[]>().ToList();
//    x.ServerAddresses = serverAddresses;
//    x.EndPoint = configuration["NacosConfig:EndPoint"];
//    x.Namespace = configuration["NacosConfig:Namespace"];
//    x.NamingUseRpc = configuration.GetValue<bool>("NacosConfig:NamingUseRpc");
//});
// nacos�������ģ���ʽһ��
builder.Services.AddNacosV2Config(builder.Configuration, sectionName: "NacosConfig");
builder.Services.Configure<UserInfo>(configuration.GetSection("UserInfo"));
// nacos�������ģ���ʽ����
//builder.Services.AddNacosV2Config(x =>
//{
//    x.ServerAddresses = new List<string> { "http://yxchatapi.xhnbzdl.cn:8848/" };
//    x.EndPoint = "";
//    x.Namespace = "public";
//    x.UserName = "nacos";
//    x.Password = "nacos";

//    // this sample will add the filter to encrypt the config with AES.
//    x.ConfigFilterAssemblies = new List<string> { "NacosDemoApi" };

//    // httpЭ������false��rpc����true
//    x.ConfigUseRpc = false;
//});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion ע��������� end

var app = builder.Build();

#region ����http����Ĺܵ����м�� begin
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
#endregion ����http����Ĺܵ����м�� end