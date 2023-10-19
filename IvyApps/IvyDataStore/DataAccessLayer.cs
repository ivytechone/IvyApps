namespace IvyApps.Data
{
    public class DataAccessLayer : IDataAccessLayer
    {
        private readonly string path;

        public DataAccessLayer(string path)
        {
            this.path = path;
        }

        public Stream ReadFileById(string id)
        {
            return new FileStream($"{path}/{id}", FileMode.Open);
        }

        public void WriteFile(string id, Stream dataStream)
        {
            using(var fileStream = new FileStream($"{path}/{id}", FileMode.Create))
            {
                dataStream.CopyTo(fileStream);
            }
        }
    }
}