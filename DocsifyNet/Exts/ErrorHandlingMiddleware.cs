using System.Diagnostics;

namespace DocsifyNet.Exts
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            this.next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
                if (context.Response != null)
                {
                    int code = context.Response.StatusCode;
                    if (code == 200)
                    {
                        return;
                    }
                    if (!context.Request.Path.ToString().EndsWith(".md"))
                    {
                        if (code == 404)
                        {
                            context.Request.Path = "/pages/404.html";
                            await next(context);
                            return;
                        }
                        if (code > 400)
                        {
                            context.Request.Path = "/pages/error";
                            await next(context);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }

        }


        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            //var code = HttpStatusCode.InternalServerError; // 500 if unexpected
            var code = StatusCodes.Status500InternalServerError;
            string info = "服务器内部错误，无法完成请求";
            if (ex is Exception)
            {
                code = 400;
                info = "请求错误： " + ex.Message;
            }
            else
            {
                switch (context.Response.StatusCode)
                {
                    case 401:
                        info = "没有权限";
                        break;
                    case 404:
                        info = "未找到服务";
                        break;
                    case 403:
                        info = "服务器理解请求客户端的请求，但是拒绝执行此请求";
                        break;
                    case 500:
                        info = "服务器内部错误，无法完成请求";
                        break;
                    case 502:
                        info = "请求错误";
                        break;
                    default:
                        info = ex.Message;
                        break;
                }

            }


            _logger.LogError(info); // todo:可记录日志,如通过注入Nlog等三方日志组件记录

            var result = System.Text.Json.JsonSerializer.Serialize(new { Coede = code.ToString(), Message = info });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = code;
            return context.Response.WriteAsync(result);
        }
    }
}
