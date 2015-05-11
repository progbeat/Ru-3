using System;
using System.Collections.Generic;
using System.Linq;

class MapStat : Dictionary<string, int>
{
    public MapStat(Func<string, string> map)
    {
        foreach (var entry in Data.Vocabulary)
        {
            string key = map(entry.Word);
            int frequency = 0;
            TryGetValue(key, out frequency);
            frequency += entry.Frequency;
            this[key] = frequency;
        }
    }

    public char[] CharsAt(int i)
    {
        var res = new List<char>();
        foreach (var kv in this)
            if (i < kv.Key.Length)
                res.Add(kv.Key[i]);
        return res.Distinct().ToArray();
    }
}