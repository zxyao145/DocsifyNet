using DocsifyNet.Module;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocsifyNet.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SidebarController : ControllerBase
    {
        private readonly ILogger<SidebarController> _logger;
        public SidebarController(ILogger<SidebarController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Gen(
            [FromServices] SidebarCreator sidebarCreator
            )
        {
            try
            {
                bool res = await sidebarCreator.RunAsync();
                return Ok(new ApiResult<bool>()
                {
                    Code = res ? 0 : 1,
                    Msg = res ? "OK" : "执行失败",
                    Data = res,
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e,"error");
                return Ok(new ApiResult<bool>()
                {
                    Code = 1,
                    Msg = e.Message,
                });
            }
        }

    }
}
