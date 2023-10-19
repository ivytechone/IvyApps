using IvyApps.Data;

namespace Trend.Models
{
public class Record
{
    public string? Date {get;set;}
    public double Weight {get;set;}
}

public class TrendUserModel : IIvyFile
{
    private readonly static int BLOCKSIZE = 31;
    private readonly string _userId;
    private Dictionary<string, int[]> _dataBlocks;
    private readonly object _mutex = new object();

    public TrendUserModel()
    {
        _userId = string.Empty;
        _dataBlocks = new Dictionary<string, int[]>();;
    }

    public TrendUserModel(string userid)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            throw new ArgumentException("userid");
        }
        _userId = userid;
        _dataBlocks = new Dictionary<string, int[]>();
    }

    public string Id => _userId;

    public IIvyFile Deserialize(string id, Stream dataStream) 
    {
        var b = dataStream.ReadByte();

        // Read file header
        if (0xCD != b ||
            0xC3 != dataStream.ReadByte() ||
            0xB1 != dataStream.ReadByte())
        {
            throw new Exception($"file header invalid {b}");
        }

        var file = new TrendUserModel(id);

        while(true)
        {
            // Read block header
            var blockType = dataStream.ReadByte();
            if (blockType == -1)
            {
                break;
            }
            var blockSize = dataStream.ReadByte();
            if (blockSize == -1)
            {
                throw new Exception("Error reading block");
            }
            var blockData = new byte[blockSize];
            dataStream.ReadExactly(blockData, 0, blockSize);

            switch (blockType)
            {
                case 0x00:
                    int year = blockData[0] << 4;
                    year += (blockData[1] & 0b11110000) >> 4;
                    int month = blockData[1] & 0b1111;
                    var daysInBlock = (blockSize-2)/2;
                    var block = new int[daysInBlock];
                    
                    for (int i = 0; i < daysInBlock; i++)
                    {
                        block[i] = blockData[2*i+2] << 8;
                        block[i] += blockData[2*i+3];
                    }

                    file._dataBlocks.Add($"{year}_{month}", block);
                    break;

                default:
                    throw new Exception("Unknown block type");
            }
        }
        return file;
    }

    public Stream Serialize()
    {
        lock (_mutex)
        {
            UInt16 size = 3;
            UInt16 blockCount = checked((UInt16)_dataBlocks.Count());
            size += checked((UInt16)(blockCount * 0x42));
            var data = new byte[size];

            // file header
            int i = 0;
            data[i++] = 0xCD;
            data[i++] = 0xC3;
            data[i++] = 0xB1;

            foreach (var blockId in _dataBlocks.Keys)
            {
                //block header
                data[i++] = 0x00;  // block type
                data[i++] = 0x40; // block size 2 + (31 * 2)

                var blockIdSplit = blockId.Split('_');
                if (blockIdSplit.Length != 2)
                {
                    throw new Exception("internal error 0x0001");
                }

                var year = Convert.ToUInt16(blockIdSplit[0]) << 4;
                var month = Convert.ToUInt16(blockIdSplit[1]);
                
                // need to pack month and year into 16 bits
                if ((month & year) != 0)
                {
                    throw new Exception("internal error 0x0002");
                }

                var x = year + month;
                data[i++] = (byte)(x >> 8);
                data[i++] = (byte)(x);

                var block = _dataBlocks[blockId];
                for (int d = 0; d < BLOCKSIZE; d++)
                {
                    data[i++] = (byte)(block[d] >> 8);
                    data[i++] = (byte)(block[d]);
                }
            }
            return new MemoryStream(data);
        }
    }

    public TrendPreferences? Preferences { get; set; }

    public IEnumerable<Record> Records {
        get {
            lock (_mutex)
            {
                foreach (var blockId in _dataBlocks.Keys)
                {
                    var block = _dataBlocks[blockId];
                    
                    for (int day = 0; day < BLOCKSIZE; day++)
                    {
                        var weight = block[day];

                        if(weight > 0)
                        {
                            yield return new Record()
                            {
                                Date = $"{blockId}_{day+1}",
                                Weight = weight / 10.0
                            };
                        }
                    }
                }
            }
        }
    }
    public bool RecordWeightToday(int weight)
    {
        lock(_mutex)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var currentLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
      
            var block = getDataBlock(currentLocalTime.Year, currentLocalTime.Month);
            block[currentLocalTime.Day - 1] = weight;
            return true;
        }
    }

    public bool RecordWeight(int year, int month, int day, int weight)
    {
        var block = getDataBlock(year, month);
        block[day - 1] = weight;
        return true;
    }

    public WeightRecord WeightRecordToday()
    {
        lock(_mutex)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var currentLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            
            var block = getDataBlock(currentLocalTime.Year, currentLocalTime.Month);
            
            return new WeightRecord() 
            {
                Year = currentLocalTime.Year,
                Month = currentLocalTime.Month,
                Day = currentLocalTime.Day,
                Weight =  block[currentLocalTime.Day - 1]
            };
        }
    }

    private int[] getDataBlock(int year, int month)
    {
        lock (_mutex)
        {
            if (!_dataBlocks.TryGetValue($"{year}_{month}", out int[]? block))
            {
                block = new int[BLOCKSIZE];
                _dataBlocks.Add($"{year}_{month}", block);
            }
            return block;
        }
    }
}
}

