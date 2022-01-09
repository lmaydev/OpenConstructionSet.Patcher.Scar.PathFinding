using System;
using System.Collections.Generic;

namespace OpenConstructionSet.Patcher.Scar.PathFinding
{
    internal static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }
    }
}
