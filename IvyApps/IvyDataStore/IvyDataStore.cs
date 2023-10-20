using System.Collections.Concurrent;

namespace IvyApps.Data
{

public class ReadRequest<T> where T: IIvyFile
{
    private readonly string _id;
    private volatile TaskCompletionSource<T> _tcs;
    public ReadRequest(string id)
    {
        _id = id;
        _tcs = new TaskCompletionSource<T>();
    }

    public string Id => _id;

    public Task<T> WaitAsync()
    {
        return _tcs.Task;
    }

    public void Complete(T file)
    {
        _tcs.SetResult(file);
    }

    public void Fail(Exception ex)
    {
        _tcs.SetException(ex);
    }
}

public class IvyDataStore<T> : IIvyDataStore<T> where T: IIvyFile, new()
{
    private readonly Dictionary<string, T> fileDictionary;
    private readonly ConcurrentQueue<T> writeQueue;
    private readonly ConcurrentQueue<ReadRequest<T>> readQueue;
    private readonly Thread workerThread;
    private readonly T fileSingleton;
    private readonly EventWaitHandle shutDownEvent;
    private readonly IDataAccessLayer dataAccessLayer;
    private TaskCompletionSource? stopTcs;
    private bool running;
    private bool shuttingDown;

    public IvyDataStore(IvyDataStoreConfig? config, string storeName, IDataAccessLayer? dataAccess = null)
    {
        if (config == null ||
            string.IsNullOrWhiteSpace(config.Path))
        {
            throw new Exception("IvyDataStoreConfig missing");
        }

        fileDictionary = new Dictionary<string, T>();
        writeQueue = new ConcurrentQueue<T>();
        readQueue = new ConcurrentQueue<ReadRequest<T>>();
        fileSingleton = new T();
        running = false;
        shuttingDown = false;
        dataAccessLayer = dataAccess ?? new DataAccessLayer(Path.Combine(config.Path, storeName));
        shutDownEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        workerThread = new Thread(new ParameterizedThreadStart(Worker));
    }

    public void Start()
    {
        if (running)
        {
            throw new Exception ("Already running");
        }
        else if (shuttingDown)
        {
            throw new Exception ("Already stopping");
        }
        running = true;
        shutDownEvent.Reset();
        workerThread.Start();
    }

    public Task StopAsync()
    {
        if (shuttingDown)
        {
            throw new Exception("Already stopping");
        }
        else if (running)
        {
            stopTcs = new TaskCompletionSource();
            shuttingDown = true;
            shutDownEvent.Set();
            return stopTcs.Task;
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    public void Write(T file)
    {
        fileDictionary[file.Id] = file;
        writeQueue.Enqueue(file);
    }

    public async Task<T> ReadAsync(string id)
    {
        if (fileDictionary.TryGetValue(id, out var file))
        {
            return file;
        }
        var request = new ReadRequest<T>(id);
        readQueue.Enqueue(request);

        return await request.WaitAsync();
    }

    public bool IsShuttingDown => shuttingDown;

    private void Worker(object? parameters)
    {
        while(!shuttingDown)
        {
            while(readQueue.TryDequeue(out var readRequest))
            {
                try
                {
                    using (var dataStream = dataAccessLayer.ReadFileById(readRequest.Id))
                    { 
                        readRequest.Complete((T)fileSingleton.Deserialize(readRequest.Id, dataStream));
                    }
                }
                catch(Exception ex)
                {
                    readRequest.Fail(ex);
                }
            }

            while (writeQueue.TryDequeue(out var file))
            {
                if (file != null)
                {
                    var fileData = file.Serialize();
                    dataAccessLayer.WriteFile(file.Id, fileData);
                }
                else
                {
                    shutDownEvent.WaitOne(5000);
                }
            }
        }

        shuttingDown = false;
        running = false;
        stopTcs?.SetResult();
    }
}
}