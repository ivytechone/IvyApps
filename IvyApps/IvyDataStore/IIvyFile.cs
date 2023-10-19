namespace IvyApps.Data
{
public interface IIvyFile
{
    string Id { get; }
    public Stream Serialize();
    public IIvyFile Deserialize(string id, Stream dataStream);
}
}