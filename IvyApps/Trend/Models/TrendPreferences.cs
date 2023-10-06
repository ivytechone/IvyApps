namespace Trend.Models
{
public enum Units
{
    lbs = 0,
    kg
}

public class TrendPreferences
{
    public Units? PreferredUnits {get; set;}
}
}