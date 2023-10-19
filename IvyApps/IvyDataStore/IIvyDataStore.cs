namespace IvyApps.Data
{
    public interface IIvyDataStore<T>
    {
        void Start();
        Task StopAsync();
        Task<T> ReadAsync(string id);
        void Write(T file);
        bool IsShuttingDown{ get; }
    }
}