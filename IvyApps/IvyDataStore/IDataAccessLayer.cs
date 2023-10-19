namespace IvyApps.Data
{
    public interface IDataAccessLayer
    {
        Stream ReadFileById(string id);
        void WriteFile(string id, Stream dataStream);
    }
}