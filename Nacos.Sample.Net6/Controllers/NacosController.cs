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
        private readonly INacosConfigService _svc;

        public NacosController(INacosConfigService svc)
        {
            _svc = svc;
        }
        [HttpGet]
        public async Task<string> GetSectionAsync(string dataId)
        {
            var res = await _svc.GetConfig(dataId, "nacos_demo", 3000);
            return res;
        }
        [HttpGet]
        public async Task<string> GetDBConnectionString()
        {
            return await _svc.GetConfig("DataBase_ConnectionString", "nacos_demo", 3000);
        }
    }
}