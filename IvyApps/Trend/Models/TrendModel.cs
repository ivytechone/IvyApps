using System.Collections.Concurrent;
using System.Security.Cryptography;
using IvyApps.Config;

namespace Trend.Models
{
public class TrendModel : ITrendModel
{
    private ConcurrentDictionary<string, TrendUserModel> _userModels;
    private readonly TrendConfig _trendConfig;

    public TrendModel(TrendConfig? trendConfig)
    {
        if (trendConfig == null || string.IsNullOrWhiteSpace(trendConfig.DataLocation))
        {
            throw new Exception("TrendConfig missing or invalid.");
        }

        _userModels = new ConcurrentDictionary<string, TrendUserModel>();
        _trendConfig = trendConfig;
    }

    public async Task<TrendUserModel?> GetUserModel(IvyIdentity identity)
    {
        if (_userModels.TryGetValue(identity.Id, out var model))
        {
            return model;
        }

        // todo dispach to load from disk.


        model = new TrendUserModel(){
            Preferences = new TrendPreferences() {
                PreferredUnits = Units.lbs
            }
        };

        if (!_userModels.TryAdd(identity.Id, model))
        {
            return null;
        }

        return await Task.FromResult<TrendUserModel?>(model);
    }

    public bool Save()
    {
        bool success = true;
        var toSave = new List<string>();
        toSave.AddRange(_userModels.Keys);

        foreach(var userId in toSave)
        {
            try 
            {
                var userModel = _userModels[userId];
                var data = userModel.Serialize();
                var fs = new FileStream($"{_trendConfig.DataLocation}/{userId}", FileMode.Create);
                var w = new BinaryWriter(fs);
                w.Write(data);
                w.Close();
                fs.Close();
            } 
            catch (Exception ex)
            {
                success = false;
                Console.WriteLine(ex.ToString());
            }
        }
        return success;
    }
}
}
