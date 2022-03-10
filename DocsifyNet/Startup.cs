using DocsifyNet.Exts;
using DocsifyNet.Module;
using Microsoft.AspNetCore.HttpOverrides;

namespace DocsifyNet
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            });

            services.AddRazorPages();
            services.Configure<SidebarCreatorOption>(Configuration.GetSection("SidebarCreatorOption"));
            services.AddSingleton<SidebarCreator>();

        }

        // This method gets called by the runtime.
        // Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
            IWebHostEnvironment env,
            ILogger<Startup> logger,
            IHostApplicationLifetime lifetime)
        {
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/pages/error");
            }

            // app.UseStatusCodePages(System.Net.Mime.MediaTypeNames.Text.Plain, "Status Code Page: {0}");

            // 全局异常捕获
            app.UseMiddleware<ErrorHandlingMiddleware>();

            ConfigHttps(app);

            app.UseStaticFiles();
            app.UseFileServer();
            app.UseRouting();
            app.UseAuthorization();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor
                                  | ForwardedHeaders.XForwardedProto
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            _ = WatchDocsDir(app, logger, lifetime);
        }

        /// <summary>
        /// 配置https相关信息
        /// </summary>
        void ConfigHttps(IApplicationBuilder webApp)
        {
            var hsts = Configuration.GetValue<bool?>("Hsts");
            if (hsts == true)
            {
                webApp.UseHsts();
                webApp.UseHttpsRedirection();
            }
            else
            {
                var httpsRedirection = Configuration.GetValue<bool?>("HttpsRedirection");
                if (httpsRedirection == true)
                {
                    webApp.UseHttpsRedirection();
                }
            }
        }


        async Task WatchDocsDir(IApplicationBuilder webApp, ILogger<Startup> logger, IHostApplicationLifetime lifetime)
        {
            DateTime lastRenGen = DateTime.Now;
            async void ReGenSideBar(object sender, FileSystemEventArgs e)
            {
                if ((DateTime.Now - lastRenGen) < TimeSpan.FromSeconds(3))
                {
                    return;
                }
                lastRenGen = DateTime.Now;

                var fileName = Path.GetFileNameWithoutExtension(e.FullPath);
                var dir = Path.GetDirectoryName(e.FullPath);


                if (fileName.StartsWith("_"))
                {
                    return;
                }

                if (dir != null &&
                    (dir.StartsWith(".") || dir.StartsWith("_")))
                {
                    return;
                }

                logger.LogInformation("ReGen _sidebar.md, WatcherChangeTypes:{ChangeType}, Name:{Name}, FullPath:{FullPath}"
                    , e.ChangeType, e.Name, e.FullPath);
                await webApp.ApplicationServices.GetRequiredService<SidebarCreator>().RunAsync();
            }

            await webApp.ApplicationServices.GetRequiredService<SidebarCreator>().RunAsync();


            var env = Environment;
            var homeDir = Path.Combine(env.WebRootPath, "docs");
            logger.LogInformation("_sidebar.md ReGen, watch dir:{homeDir}"
            , homeDir);

            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(homeDir);
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.Created += ReGenSideBar; ;
            fileSystemWatcher.Changed += ReGenSideBar;
            fileSystemWatcher.Deleted += ReGenSideBar;
            fileSystemWatcher.Renamed += ReGenSideBar;
            fileSystemWatcher.EnableRaisingEvents = true;

            lifetime.ApplicationStopping.Register(() =>
            {
                fileSystemWatcher.EnableRaisingEvents = false;
                fileSystemWatcher.Created -= ReGenSideBar; ;
                fileSystemWatcher.Changed -= ReGenSideBar;
                fileSystemWatcher.Deleted -= ReGenSideBar;
                fileSystemWatcher.Renamed -= ReGenSideBar;
            });
        }
    }
}
