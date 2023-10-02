using IvyApps.Interfaces;
using IvyApps.Config;
using JWT.Algorithms;
using JWT.Builder;
using System.Security.Cryptography.X509Certificates;

namespace IvyApps
{
    public class IvyAuth : IIvyAuth
    {
        private readonly IvyAuthConfig _config;

        public IvyAuth(IvyAuthConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.certPem) || string.IsNullOrWhiteSpace(config.authServerUrl) )
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

        public bool IsTokenValid(string token)
        {
            var cert = X509Certificate2.CreateFromPem(_config.certPem);

            string decodedToken;
            try
            {
                decodedToken = JwtBuilder.Create()
                    .WithAlgorithm(new RS256Algorithm(cert))
                    .MustVerifySignature()
                    .Decode(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }
    }   
}
