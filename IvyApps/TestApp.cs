using IvyTech.Auth;
using Microsoft.AspNetCore.Mvc;

namespace IvyApps
{ 
    public static class TestApp
    {
        static string appRoot = "TestApp";
        static string clientId = "5967CA44-7FE7-462E-8DC3-1E2024F664AA";

        public static WebApplication AddTestAppMapping(this WebApplication app)
        {
            app.MapGet($"{appRoot}/test", Test);
            app.MapGet($"{appRoot}/logout", Logout);
            app.MapGet($"{appRoot}/identity", Identity);
            app.MapPost($"{appRoot}/token", Token);
            return app;
        }

        public static IResult Logout(HttpContext httpContext)
        {
            httpContext.Response.Cookies.Append("ivyauth", "deleted", new CookieOptions() { Secure = true, HttpOnly = true, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UnixEpoch });
            return Results.Redirect("/");
        }

        public static IResult Identity(IIvyAuth ivyAuth, HttpContext httpContext)
        {
            var identity = ivyAuth.GetIdentity(httpContext.Request);
            if (identity != null)
            {
                return Results.Ok(identity);
            }
            else
            {
                return Results.Unauthorized();
            }
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

        public static IResult Test(IIvyAuth ivyAuth, HttpContext httpContext)
        {
            var cookie = httpContext.Request.Cookies["ivyauth"];

            if (!string.IsNullOrWhiteSpace(cookie) && ivyAuth.IsTokenValid(cookie))
            {
                return Results.Ok(new { message = "This is the secret message." });
            }

            return Results.Unauthorized();
        }
    }
}
