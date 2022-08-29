#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP_2_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;

namespace Palmmedia.ReportGenerator.Core.Common
{
    /// <summary>
    /// Extension methods on <see cref="IEnumerable{T}"/>.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Returns a new enumerable collection that contains the last
        /// <paramref name="count"/> elements from <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the enumerable collection.</typeparam>
        /// <param name="source">An enumerable collection instance.</param>
        /// <param name="count">The number of elements to take from the end of the collection.</param>
        /// <returns>A new enumerable collection that contains the last
        /// <paramref name="count"/> elements from <paramref name="source"/>.</returns>
        /// <remarks>
        /// If <paramref name="count"/> is not a positive number, this method returns an empty enumerable collection.
        /// </remarks>
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (count <= 0)
            {
                return Enumerable.Empty<T>();
            }

            // Check if we have direct access to elements
            if (source is IList<T> list)
            {
                if (list.Count <= count)
                {
                    return list;
                }
                else
                {
                    return TakeLast(list, count);
                }
            }

            return KeepElementByCount(source, count);
        }

        private static IEnumerable<T> KeepElementByCount<T>(this IEnumerable<T> source, int count)
        {
            var queue = new Queue<T>(count);
            foreach (var item in source)
            {
                if (queue.Count == count)
                {
                    queue.Dequeue();
                }

                queue.Enqueue(item);
            }
            return queue;
        }

        private static IEnumerable<T> TakeLast<T>(IList<T> list, int count)
        {
            for (int i = list.Count - count; i < list.Count; i++)
            {
                yield return list[i];
            }
        }
    }
}

#endif
