using DocsifyNet.Module;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<SidebarCreatorOption>(builder.Configuration.GetSection("SidebarCreatorOption"));
builder.Services.AddSingleton<SidebarCreator>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseFileServer();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

await app.Services.GetRequiredService<SidebarCreator>().RunAsync();

app.Run();

