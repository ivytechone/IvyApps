public class IvyIdentity 
{
    private string _name;
    private string _id;
    private IvyIdentity(string name, string id)
    {
        _name = name;
        _id = id;
    }

    public static IvyIdentity? FromToken(IvyAuthToken token)
    {
        if (token == null ||
            string.IsNullOrWhiteSpace(token.name) ||
            string.IsNullOrWhiteSpace(token.sub))
        {
            return null;
        }
        
        return new IvyIdentity(token.name, token.sub);
    }

    public string Name {
        get {
            return _name;
        }
    }

    public string Id {
        get {
            return _id;
        }
    }
}