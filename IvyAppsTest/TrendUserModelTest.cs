using Microsoft.VisualStudio.TestTools.UnitTesting;
using IvyApps.Data;
using Trend.Models;

namespace IvyApps.Test
{
    [TestClass]
    public class TrendUserModelTest
    {
        private readonly IIvyFile fileSingleton;

        public TrendUserModelTest()
        {
            fileSingleton = new TrendUserModel();
        }

        [TestMethod]
        public void SingleDataBlockSaveLoad()
        {
            var model = new TrendUserModel("1");
            model.RecordWeight(2023, 10, 19, 1005);

            var dataStream = model.Serialize();

            var loadedFile = (TrendUserModel?)fileSingleton.Deserialize("1", dataStream);
            var record = loadedFile?.Records.Where(x => x.Date.Year.Equals(2023) && x.Date.Month.Equals(10) && x.Date.Day.Equals(19)).FirstOrDefault();
            Assert.AreEqual(record?.Weight, 100.5);
        }

         [TestMethod]
        public void SecondDataBlockSaveLoad()
        {
            var model = new TrendUserModel("1");
            model.RecordWeight(2023, 10, 19, 1005);
            model.RecordWeight(2023, 11, 1, 1015);

            var dataStream = model.Serialize();

            var loadedFile = (TrendUserModel?)fileSingleton.Deserialize("1", dataStream);
            var record1 = loadedFile?.Records.Where(x => x.Date.Year.Equals(2023) && x.Date.Month.Equals(10) && x.Date.Day.Equals(19)).FirstOrDefault();
            var record2 = loadedFile?.Records.Where(x => x.Date.Year.Equals(2023) && x.Date.Month.Equals(11) && x.Date.Day.Equals(1)).FirstOrDefault();
            Assert.AreEqual(100.5, record1?.Weight);
            Assert.AreEqual(101.5, record2?.Weight);
        }

        [TestMethod]
        public void RecordsSince()
        {
            var model = new TrendUserModel("1");
            model.RecordWeight(2023, 10, 19, 1005);
            model.RecordWeight(2023, 10, 18, 1015);
            var records = model.RecordsSince(new DateTime(2023, 10, 17)).Take(3).ToArray();
            Assert.AreEqual(3, records.Length);
            Assert.AreEqual(0, records[0].Weight);
            Assert.AreEqual(1015, records[1].Weight);
            Assert.AreEqual(1005, records[2].Weight);
        }

        [TestMethod]
        public void RecordsSinceSpanMonth()
        {
            var model = new TrendUserModel("1");
            model.RecordWeight(2023, 10, 19, 1005);
            model.RecordWeight(2023, 10, 18, 1015);
            model.RecordWeight(2023, 11, 1, 1075);
            var records = model.RecordsSince(new DateTime(2023, 10, 17)).Take(30).ToArray();
            Assert.AreEqual(30, records.Length);
            Assert.AreEqual(0, records[0].Weight);
            Assert.AreEqual(1015, records[1].Weight);
            Assert.AreEqual(1005, records[2].Weight);
            Assert.AreEqual(11, records[15].Date.Month);
            Assert.AreEqual(1, records[15].Date.Day);
            Assert.AreEqual(1075, records[15].Weight);
        }

        [TestMethod]
        public void RecordsSinceSpanYear()
        {
            var model = new TrendUserModel("1");
            model.RecordWeight(2023, 12, 31, 1005);
            model.RecordWeight(2024, 1, 1, 1015);
            var records = model.RecordsSince(new DateTime(2023, 12, 31)).Take(2).ToArray();
            Assert.AreEqual(2, records.Length);
            Assert.AreEqual(1005, records[0].Weight);
            Assert.AreEqual(1015, records[1].Weight);
        }
    }
}