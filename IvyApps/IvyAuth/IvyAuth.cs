using IvyApps.Config;
using Newtonsoft.Json;
using JWT.Algorithms;
using JWT.Builder;
using System.Security.Cryptography.X509Certificates;

namespace IvyTech.Auth
{
    public class IvyAuth : IIvyAuth
    {
        private readonly IvyAuthConfig _config;

        public IvyAuth(IvyAuthConfig? config)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.certPem) || string.IsNullOrWhiteSpace(config.authServerUrl) )
            {
                throw new Exception("IvyAuthConfig is missing or invalid");
            }
            _config = config;
        }

        public async Task<string?> GetToken(string clientId, string code, string code_verifier)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("code_verifier", code_verifier),
            };

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_config.authServerUrl}/token")
            {
                Content = new FormUrlEncodedContent(data)
            };

            var res = await client.SendAsync(request);

            if (res.IsSuccessStatusCode)
            {
                var token = await res.Content.ReadAsStringAsync();
                return token;
            }
            else
            {
                return null;
            }
        }

        public IvyIdentity? GetIdentity(HttpRequest request)
        {
            var rawToken = request.Cookies["ivyauth"];
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                return null;
            }

            var token = ValidateAndDecodeToken(rawToken);
            if (token == null) {
                return null;
            }

            return IvyIdentity.FromToken(token);
        }

        public bool IsTokenValid(string token)
        {
            return ValidateAndDecodeToken(token) != null;
        }

        private IvyAuthToken? ValidateAndDecodeToken(string rawtoken)
        {
            try
            {
                var cert = X509Certificate2.CreateFromPem(_config.certPem);
                var decodedToken = JwtBuilder.Create()
                    .WithAlgorithm(new RS256Algorithm(cert))
                    .MustVerifySignature()
                    .Decode(rawtoken);

                var token = JsonConvert.DeserializeObject<IvyAuthToken>(decodedToken);

                // todo verify token.aud
    
                return token;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }   
}
