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
                if (record.Weight == 0)
                {
                    continue;
                }
                if (record.Weight > max)
                {
                    max = record.Weight;
                }
                if (record.Weight < min)
                {
                    min = record.Weight;
                }
            }

            var range = max - min;

            var prev = min;
            foreach(var record in records)
            {
                if (record.Weight == 0)
                {
                    yield return (prev - min) / range;
                }
                else
                {
                    yield return (record.Weight - min) / range;
                }
                prev = record.Weight;
            }
        }
    }
}