using IvyTech.Auth;
using Trend.Models;
using Microsoft.AspNetCore.Mvc;

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
                var model = new MainPageModel()
                {
                    Identity = identity,
                    WeightRecordToday = userModel.WeightRecordToday()
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