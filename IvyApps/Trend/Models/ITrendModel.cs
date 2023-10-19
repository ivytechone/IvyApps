namespace Trend.Models
{
public interface ITrendModel
{
    Task<TrendUserModel?> GetUserModel(IvyIdentity identity);
    void MarkDirty(TrendUserModel model);
}
}