using System.Collections.Generic;
using System.Linq;

public static class Extensions
{
    public static List<List<T>> Split<T>(this List<T> source, int amount)
    {
        int temp = (source.Count / amount);
        return source
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / temp)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }
}
