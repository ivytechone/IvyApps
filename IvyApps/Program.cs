using IvyApps;
using IvyApps.Config;
using IvyApps.Data;
using IvyTech.Auth;
using IvyTech.IvyLogging.Extensions;
using IvyTech.IvyWebApi;
using IvyTech.Logging;
using Trend.Models;

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.UseIvyWebApi("IvyApps", AppInfo.Version);
    builder.AddIvyDebugLogger();

    var ivyAuth = new IvyAuth(builder.Configuration.GetSection("IvyAuth").Get<IvyAuthConfig>());
    var trendDataStore = new IvyDataStore<TrendUserModel>(builder.Configuration.GetSection("IvyDataStore").Get<IvyDataStoreConfig>(), "trend");
    var trendModel = new TrendModel(trendDataStore);
  
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddMvc();
    builder.Services.AddControllers();
    builder.Services.AddSingleton<IIvyAuth>(x => ivyAuth);
    builder.Services.AddSingleton<ITrendModel>(x => trendModel);
    builder.Services.AddSingleton<IIvyDataStore<TrendUserModel>>(x => trendDataStore);
    builder.Services.AddHostedService<IvyDataStoreHost<TrendUserModel>>();
    var app = builder.Build();
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseRouting();
    app.AddPing();
    app.AddTestAppMapping();
    app.AddTrendAppMapping();
    app.MapControllers();
    app.UseIvyLogging();
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    DebugLogger.Logger?.Fatal("Unhandled Excpetion", ex);
}