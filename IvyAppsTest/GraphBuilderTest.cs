using IvyApps.Trend;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trend.Models;

namespace IvyApps.Test
{
    [TestClass]
    public class GraphBuilderTest
    {
        [TestMethod]
        public void BasicGraphPoints()
        {
            var model = new TrendUserModel("1");
            model.RecordWeight(2023, 10, 18, 1000);
            model.RecordWeight(2023, 10, 19, 1010);
            model.RecordWeight(2023, 10, 20, 1020);

            var result = GraphBuilder.BasicGraphPoints(model, new DateTime(2023, 10, 18), 3).ToArray();
            Assert.AreEqual(0, result[0]);
            Assert.AreEqual(0.5, result[1]);
            Assert.AreEqual(1.0, result[2]);

        }
    }
}