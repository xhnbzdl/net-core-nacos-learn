
using Nacos.V2.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

#region ע��������� begin
// nacos����ע�ᣬ��������
//builder.Services.AddNacosAspNet(configuration, "NacosConfig");
// nacos�������ģ���ʽһ��
builder.Services.AddNacosV2Config(builder.Configuration, sectionName: "NacosConfig");
// nocas�������ģ���ʽ����
//builder.Services.AddNacosV2Config(x =>
//{
//    x.ServerAddresses = new List<string> { "http://yxchatapi.xhnbzdl.cn:8848/" };
//    x.EndPoint = "";
//    x.Namespace = "public";
//    x.UserName = "nacos";
//    x.Password = "nacos";

//    // this sample will add the filter to encrypt the config with AES.
//    x.ConfigFilterAssemblies = new List<string> { "BaseApi" };

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