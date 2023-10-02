using IvyApps;
using IvyApps.Config;
using IvyApps.Interfaces;
using IvyTech.IvyLogging.Extensions;
using IvyTech.IvyWebApi;
using IvyTech.Logging;

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.UseIvyWebApi("IvyApps", AppInfo.Version);
    builder.AddIvyDebugLogger();

    var ivyAuth = new IvyAuth(builder.Configuration.GetSection("IvyAuth").Get<IvyAuthConfig>());

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSingleton<IIvyAuth>(x => ivyAuth);
    var app = builder.Build();
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseRouting();
    app.AddPing();
    app.AddTestAppMapping();
    app.UseIvyLogging();
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    DebugLogger.Logger?.Fatal("Unhandled Excpetion", ex);
}