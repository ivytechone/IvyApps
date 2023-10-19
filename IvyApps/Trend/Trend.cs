using IvyTech.Auth;
using Trend.Models;
using Microsoft.AspNetCore.Mvc;
using IvyApps.Data;

namespace IvyApps
{ 
    public static class TrendApp
    {
        private static string appRoot = "TrendApp";
        private static string clientId = "6CFBDF3B-C753-4D2C-AE57-F009FD7DE33A";

        public static WebApplication AddTrendAppMapping(this WebApplication app)
        {
            app.MapGet($"{appRoot}/logout", Logout);
            app.MapPost($"{appRoot}/token", Token);
            app.MapPost($"{appRoot}/recordCurrentWeight", RecordCurrentWeight);
            app.MapGet($"{appRoot}/allData", AllData);
            return app;
        }

        public static IResult Logout(HttpContext httpContext)
        {
            httpContext.Response.Cookies.Append("ivyauth", "deleted", new CookieOptions() { Secure = true, HttpOnly = true, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UnixEpoch });
            return Results.Redirect("/");
        }

        public static async Task<IResult> AllData(IIvyAuth ivyAuth, ITrendModel model, HttpContext httpContext)
        {
            var identity = ivyAuth.GetIdentity(httpContext.Request);

            if (identity == null)
            {
                return Results.Unauthorized();
            }

            var userModel = await model.GetUserModel(identity);
            if (userModel != null)
            {
                return Results.Ok(userModel);
            }

            return Results.StatusCode(500);
        }

        public class RecordCurrentWeightBody
        {
            public int? Weight {get; set;}
        }

        public static async Task<IResult> RecordCurrentWeight(IIvyAuth ivyAuth, ITrendModel model, HttpContext httpContext, [FromBody] RecordCurrentWeightBody body)
        {
            if (body == null ||
                body.Weight == null ||
                body.Weight <= 0)
            {
                return Results.BadRequest();
            }

            var identity = ivyAuth.GetIdentity(httpContext.Request);

            if (identity == null)
            {
                return Results.Unauthorized();
            }

            var userModel = await model.GetUserModel(identity);

            if (userModel != null && userModel.RecordWeightToday(body.Weight.Value)) 
            {
                model.MarkDirty(userModel);
                return Results.Ok();
            }

            return Results.StatusCode(500);
        }

        public class TokenBody
        {
            public string? code { get; set; }
            public string? code_verifier { get; set; }
        }

        public static async Task<IResult> Token(IIvyAuth ivyAuth, HttpContext httpContext, [FromBody] TokenBody tokenBody) 
        {
            if (string.IsNullOrWhiteSpace(tokenBody.code) || string.IsNullOrWhiteSpace(tokenBody.code_verifier))
            {
                return Results.BadRequest();
            }

            var token = await ivyAuth.GetToken(clientId, tokenBody.code, tokenBody.code_verifier);

            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }
            else
            {
                httpContext.Response.Cookies.Append("ivyauth", token, new CookieOptions() { HttpOnly = true, Secure = true, Path = "/", SameSite = SameSiteMode.None, MaxAge = TimeSpan.FromMinutes(3600) });
                return Results.Ok();
            }
        }
    }
}