using Arinc424;
using Arinc424.Building;

namespace NavData.Services;

public class CifpService
{
    public CifpService(IEnumerable<string> strings)
    {
        var meta = Meta424.Create(Supplement.V18);
        Data = Data424.Create(meta, strings, out var skipped, out var invalid);
        Invalid = invalid;
        Skipped = skipped;
    }

    public Data424 Data { get; }
    public Queue<Build> Invalid { get; private set; }
    public Queue<string> Skipped { get; private set; }
}