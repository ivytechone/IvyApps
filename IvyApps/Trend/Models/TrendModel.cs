using IvyApps.Config;
using IvyApps.Data;

namespace Trend.Models
{
public class TrendModel : ITrendModel
{
    private readonly IIvyDataStore<TrendUserModel> _trendDataStore;

    public TrendModel(IIvyDataStore<TrendUserModel> trendDataStore)
    {
        _trendDataStore = trendDataStore;
    }

    public async Task<TrendUserModel?> GetUserModel(IvyIdentity identity)
    {
        try
        {
            return await _trendDataStore.ReadAsync(identity.Id);
        }
        catch (Exception)
        {
            // If model does not exist, create one
            var model = new TrendUserModel(identity.Id){
                Preferences = new TrendPreferences() {
                    PreferredUnits = Units.lbs
                }
            };
            _trendDataStore.Write(model);
            return model;
        }
    }

    public void MarkDirty(TrendUserModel model)
    {
        _trendDataStore.Write(model);
    }
}
}
