using Trend.Models;

public class MainPageModel
{
    public IvyIdentity? Identity {get; set;}
    public WeightRecord? WeightRecordToday {get; set;}
    public IEnumerable<double>? GraphPoints {get; set;} 
}
