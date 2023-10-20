using Microsoft.VisualStudio.TestTools.UnitTesting;
using IvyApps.Data;
using NSubstitute;
using System.Text;

namespace IvyApps.Test
{
    class TestFile : IIvyFile
    {
        private readonly string id;

        public TestFile()
        {
            this.id = string.Empty;
            this.Data = string.Empty;
            this.SerializedData = new byte[0];
        }

        public TestFile(string id)
        {
            this.id = id;
            this.Data = string.Empty;
            this.SerializedData = new byte[0];
        }

        public string Data {get; set;}

        public byte[] SerializedData {get; set;}

        public string Id => this.id;

        public IIvyFile Deserialize(string id, Stream dataStream)
        {
            try
            {
                var file = new TestFile(id);
                var reader = new StreamReader(dataStream, Encoding.UTF8);
                file.Data = reader.ReadToEnd();
                return file;
            }
            catch(Exception)
            {
                throw;
            }
        }

        public Stream Serialize()
        {
            SerializedData = Encoding.UTF8.GetBytes(Data);
            return new MemoryStream(SerializedData);
        }
    }

    [TestClass]
    public class IvyDataStoreTest
    {
        [TestMethod]
        public async Task AddFile()
        {
            var testStore = MakeTestDataStore(out var mockDataAccess);
            testStore.Start();

            var testFile = new TestFile("1")
            {
                Data = "test1"
            };
            testStore.Write(testFile);
            var result = await testStore.ReadAsync("1");

            mockDataAccess.DidNotReceiveWithAnyArgs().ReadFileById(Arg.Any<string>());
            Assert.AreEqual(result.Data, "test1");

            await testStore.StopAsync();

            mockDataAccess.Received().WriteFile("1", Arg.Any<Stream>());
        }

        [TestMethod]
        public async Task ReadFile()
        {
            var testStore = MakeTestDataStore(out var mockDataAccess);
            testStore.Start();

            using (var mockFileStream = new MemoryStream(Encoding.UTF8.GetBytes("test1")))
            {
                mockDataAccess.ReadFileById("1").Returns(mockFileStream);
                var result = await testStore.ReadAsync("1");
                Assert.AreEqual(result.Data, "test1");
                await testStore.StopAsync();
            }
        }

        private IIvyDataStore<TestFile> MakeTestDataStore(out IDataAccessLayer mockDataAccess)
        {
            IvyDataStoreConfig config = new IvyDataStoreConfig()
            {
                Path = "/"
            };
            mockDataAccess = Substitute.For<IDataAccessLayer>();
            return new IvyDataStore<TestFile>(config, "test", mockDataAccess);
        }
    }
}