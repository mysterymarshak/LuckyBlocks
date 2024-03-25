using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Utils;

namespace LuckyBlocks.Extensions;

internal static class EnumerableExtensions
{
    public static List<T> ShuffleAndGuaranteeIndexChanging<T>(this List<T> list)
    {
        var count = list.Count;
        if (count <= 1)
            return list;

        List<T> orderedList;

        do
        {
            orderedList = list
                .OrderBy(_ => SharedRandom.Instance.Next())
                .ToList();
        } while (orderedList.Any(x => orderedList.IndexOf(x) == list.IndexOf(x)));

        return orderedList;
    }

    public static List<T> Shuffle<T>(this List<T> list)
    {
        return list
            .OrderBy(x => SharedRandom.Instance.Next())
            .ToList();
    }
    
    public static T GetRandomElement<T>(this IReadOnlyList<T> list)
    {
        return list.ElementAt(SharedRandom.Instance.Next(list.Count));
    }

    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> values, int chunkSize)
    {
        return values
            .Select((x, y) => new { Index = y, Value = x })
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x.Select(v => v.Value));
    }
}