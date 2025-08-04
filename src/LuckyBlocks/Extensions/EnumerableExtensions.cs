using System;
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
            .OrderBy(_ => SharedRandom.Instance.Next())
            .ToList();
    }

    public static T GetRandomElement<T>(this IReadOnlyCollection<T> list)
    {
        return list.ElementAt(SharedRandom.Instance.Next(list.Count));
    }

    public static T GetWeightedRandomElement<T>(this IReadOnlyList<T> items, Func<T, double> weightSelector)
    {
        var totalWeight = 0d;
        var weights = new List<double>(items.Count);

        foreach (var item in items)
        {
            var weight = weightSelector(item);
            weights.Add(weight);
            totalWeight += weight;
        }

        var randomValue = SharedRandom.Instance.NextDouble() * totalWeight;

        var cumulative = 0d;
        for (var i = 0; i < items.Count; i++)
        {
            cumulative += weights[i];
            if (cumulative >= randomValue)
                return items[i];
        }

        throw new InvalidOperationException();
    }

    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> values, int chunkSize)
    {
        return values
            .Select((x, y) => new { Index = y, Value = x })
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x.Select(v => v.Value));
    }
}