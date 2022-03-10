using Serilog;

namespace DocsifyNet.Exts
{
    public static class SerilogExtensions
    {
        /// <summary>
        /// 根据配置文件名称（不带扩展名）加载指定的配置项
        /// </summary>
        /// <param name="configFileName">
        /// 配置文件名称（不带扩展名），
        /// 使用约定，配置文件放在项目的Config目录中，如logging配置：configs/logging.json
        /// </param>
        /// <returns></returns>
        public static IConfiguration? LoadJsonConfig(string configFileName)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(AppContext.BaseDirectory)
               .AddJsonFile(configFileName + ".json", true, true);

            return builder.Build();
        }


        /// <summary>
        /// 添加Serilog扩展(.net core 3)
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHostBuilder UseLoggingOfSerilog(this IHostBuilder builder)
        {
            builder.UseSerilog(
                (hostingContext, loggerConfiguration) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo
                        .Async(w => w.Console())
                        .CreateLogger();

                    var config = hostingContext.Configuration;
                    var cfg = LoadJsonConfig("logger");

                    if (cfg != null)
                    {
                        Log.Logger.Information("logger logger.json");
                        loggerConfiguration
                            .ReadFrom.Configuration(cfg);
                    }

                    Log.CloseAndFlush();
                }
            );

            return builder;
        }
    }
}
