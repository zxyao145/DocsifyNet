using DocsifyNet;
using DocsifyNet.Exts;
using DocsifyNet.Module;
using System.Diagnostics;
using System.ServiceProcess;

#region mini api
try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
    {
        var curPath = Environment.CurrentDirectory;
        var basePath = Path.GetDirectoryName(AppContext.BaseDirectory);
        bool isService = (curPath != basePath);
       
        Console.WriteLine($"is windows service: {isService}");

        if (isService)
        {
            var webApplicationOptions = new WebApplicationOptions()
            {
                ContentRootPath = AppContext.BaseDirectory,
                Args = args,
                ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName
            };
            builder = WebApplication.CreateBuilder(webApplicationOptions);
            builder.Host.UseWindowsService();
        }
    }
   
    builder.Host
        .ConfigureLogging(options => options.ClearProviders())
        .UseLoggingOfSerilog();

    // Add services to the container.
    builder.Services.AddRazorPages();
    builder.Services.Configure<SidebarCreatorOption>(builder.Configuration.GetSection("SidebarCreatorOption"));
    builder.Services.AddSingleton<SidebarCreator>();


    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    }

    // app.UseStatusCodePages(System.Net.Mime.MediaTypeNames.Text.Plain, "Status Code Page: {0}");

    // ȫ???쳣????
    app.UseMiddleware<ErrorHandlingMiddleware>();

    ConfigHttps(app);

    app.UseStaticFiles();
    app.UseFileServer();

    app.UseRouting();

    app.UseAuthorization();

    app.MapControllers();
    app.MapRazorPages();

    _ = WatchDocsDir(app);
    app.Run();

}
catch (Exception e)
{
    Console.WriteLine($"run failed: {e.Message}");
    File.WriteAllTextAsync("/DocsifyNet-ERROR!!!.txt", e.Message);
}


/// <summary>
/// ????https??????Ϣ
/// </summary>
void ConfigHttps(WebApplication webApp)
{
    var hsts = webApp.Configuration.GetValue<bool?>("Hsts");
    if (hsts == true)
    {
        webApp.UseHsts();
        webApp.UseHttpsRedirection();
    }
    else
    {
        var httpsRedirection = webApp.Configuration.GetValue<bool?>("HttpsRedirection");
        if (httpsRedirection == true)
        {
            webApp.UseHttpsRedirection();
        }
    }
}


async Task WatchDocsDir(WebApplication webApp)
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

        webApp.Logger.LogInformation("ReGen _sidebar.md, WatcherChangeTypes:{ChangeType}, Name:{Name}, FullPath:{FullPath}"
            , e.ChangeType, e.Name, e.FullPath);
        await webApp.Services.GetRequiredService<SidebarCreator>().RunAsync();
    }

    await webApp.Services.GetRequiredService<SidebarCreator>().RunAsync();


    var env = webApp.Environment;
    var homeDir = Path.Combine(env.WebRootPath, "docs");
    webApp.Logger.LogInformation("_sidebar.md ReGen, watch dir:{homeDir}"
    , homeDir);

    FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(homeDir);
    fileSystemWatcher.IncludeSubdirectories = true;
    fileSystemWatcher.Created += ReGenSideBar; ;
    fileSystemWatcher.Changed += ReGenSideBar;
    fileSystemWatcher.Deleted += ReGenSideBar;
    fileSystemWatcher.Renamed += ReGenSideBar;
    fileSystemWatcher.EnableRaisingEvents = true;

    webApp.Lifetime.ApplicationStopping.Register(() =>
    {
        fileSystemWatcher.EnableRaisingEvents = false;
        fileSystemWatcher.Created -= ReGenSideBar; ;
        fileSystemWatcher.Changed -= ReGenSideBar;
        fileSystemWatcher.Deleted -= ReGenSideBar;
        fileSystemWatcher.Renamed -= ReGenSideBar;
    });
}

#endregion

#region classic
//var hostBuilder = Host.CreateDefaultBuilder(args)
//    .UseWindowsService()
//     .ConfigureAppConfiguration((hostingContext, config) =>
//     {
//         config.AddJsonFile("appsettings.json");
//     })
//    .ConfigureLogging(options => options.ClearProviders())
//    .UseLoggingOfSerilog()
//    .ConfigureWebHostDefaults(webBuilder =>
//    {
//        webBuilder.UseStartup<Startup>();
//    });
//await hostBuilder.Build().RunAsync(); 
#endregion