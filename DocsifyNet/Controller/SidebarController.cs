using DocsifyNet.Module;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocsifyNet.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SidebarController : ControllerBase
    {
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
                return Ok(new ApiResult<bool>()
                {
                    Code = 1,
                    Msg = e.Message,
                });
            }
        }

    }
}
