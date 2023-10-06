namespace IvyTech.Auth
{
    public interface IIvyAuth
    {
        public Task<string?> GetToken(string clientId, string code, string code_verifier);
        public bool IsTokenValid(string token);
        public IvyIdentity? GetIdentity(HttpRequest request);
    }
}
