namespace Trend.Models
{
public class Record
{
    public string? Date {get;set;}
    public int Weight {get;set;}
}

public class TrendUserModel
{
    private readonly static int BLOCKSIZE = 31;
    public TrendUserModel()
    {
        _datablocks = new Dictionary<string, int[]>();
    }

  /*  public static TrendUserModel? Deserialize(byte[] data) 
    {
        if (data[0] != 0xCD || data[1] != 0xC3 || data[2] != 0xB1) 
        {
            return null;
        }

        UInt16 i = 3;
        while (true)
        {
            if ()
        }
     }  
*/
    public byte[] Serialize()
    {
        lock (_mutex)
        {
            UInt16 size = 3;
            UInt16 blockCount = checked((UInt16)_datablocks.Count());
            size += checked((UInt16)(blockCount * 0x42));

            var data = new byte[size];

            // file header
            int i = 0;
            data[i++] = 0xCD;
            data[i++] = 0xC3;
            data[i++] = 0xB1;

            foreach (var blockId in _datablocks.Keys)
            {
                //block header
                data[i++] = 0x00;
                data[i++] = 0x40; // always storing 2 byte for id and 31 x 2 bytes for data

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

                var block = _datablocks[blockId];
                for (int d = 0; d < BLOCKSIZE; d++)
                {
                    data[i++] = (byte)(block[d] >> 8);
                    data[i++] = (byte)(block[d]);
                }
            }
            return data;
        }
    }

    public TrendPreferences? Preferences { get; set; }

    public IEnumerable<Record> Records {
        get {
            lock (_mutex)
            {
                foreach (var blockId in _datablocks.Keys)
                {
                    var block = _datablocks[blockId];
                    
                    for (int day = 0; day < BLOCKSIZE; day++)
                    {
                        var weight = block[day];

                        if(weight > 0)
                        {
                            yield return new Record()
                            {
                                Date = $"{blockId}_{day+1}",
                                Weight = weight
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
        if (!_datablocks.TryGetValue($"{year}_{month}", out int[]? block))
        {
            block = new int[BLOCKSIZE];
            _datablocks.Add($"{year}_{month}", block);
        }

        return block;
    }
    
    private Dictionary<string, int[]> _datablocks;
    private readonly object _mutex = new object();
}
}

