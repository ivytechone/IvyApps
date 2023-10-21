using IvyTech.Auth;
using Trend.Models;
using Microsoft.AspNetCore.Mvc;
using IvyApps.Trend;

public class MainPageController : Controller
{
    private readonly IIvyAuth _ivyAuth;
    private readonly ITrendModel _trendModel;

    public MainPageController(IIvyAuth ivyAuth, ITrendModel trendModel)
    {
        _ivyAuth = ivyAuth;
        _trendModel = trendModel;
    }
    
    [HttpGet]
    [Route("/TrendApp/")]
    public async Task<IActionResult> MainPage()
    {
        var identity = _ivyAuth.GetIdentity(Request);

        if (identity != null)
        {
            var userModel = await _trendModel.GetUserModel(identity);

            if (userModel != null)
            {
                var today = userModel.WeightRecordToday();
                var model = new MainPageModel()
                {
                    Identity = identity,
                    WeightRecordToday = today,
                    GraphPoints = GraphBuilder.BasicGraphPoints(userModel, today.Date.AddDays(-30), 30)
                };

                return View(model);
            }
        }

        return View(null);
    }

    [HttpGet]
    [Route("/TrendApp/auth")]
    public IActionResult AuthPage()
    {
        return View();
    }
}