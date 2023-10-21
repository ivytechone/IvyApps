using Trend.Models;

namespace IvyApps.Trend
{
    public static class GraphBuilder
    {
        public static IEnumerable<double> BasicGraphPoints(TrendUserModel model, DateTime start, int days)
        {
            var records = model.RecordsSince(start).Take(days).ToArray();
            var min = records[0].Weight;
            var max = records[0].Weight;

            foreach (var record in records)
            {
                if (record.Weight > max)
                {
                    max = record.Weight;
                }
            }

            var range = max - min;

            foreach(var record in records)
            {
                yield return (record.Weight - min) / range;
            }
        }
    }
}