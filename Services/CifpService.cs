using Arinc424;
using Arinc424.Building;

namespace NavData.Services;

public class CifpService
{
    public Data424? Data { get; private set; }
    public Queue<Build>? Invalid { get; private set; }
    public Queue<string>? Skipped { get; private set; }

    public bool UpdateCifp(IAsyncEnumerable<string> strings)
    {
        var stringsList = strings.ToBlockingEnumerable().ToList();
        if (stringsList.Count <= 0)
        {
            return false;
        }

        var meta = Meta424.Create(Supplement.V18);
        Data = Data424.Create(meta, stringsList, out var skipped, out var invalid);
        Invalid = invalid;
        Skipped = skipped;
        return true;
    }
}
