namespace Trend.Models
{
public interface ITrendModel
{
    Task<TrendUserModel?> GetUserModel(IvyIdentity identity);
    bool Save();
}
}