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
    }
}