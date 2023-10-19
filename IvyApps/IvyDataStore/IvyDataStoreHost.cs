namespace IvyApps.Data
{
    public sealed class IvyDataStoreHost<T> : IHostedService
    {
        private readonly IIvyDataStore<T> _ivyDataStore;
        public IvyDataStoreHost(IIvyDataStore<T> fileSystemDb)
        {
            _ivyDataStore = fileSystemDb;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ivyDataStore.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _ivyDataStore.StopAsync();
        }
    }
}