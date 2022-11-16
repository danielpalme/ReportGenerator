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
        /// Computes the sum of the sequence of System.Int32 values that are obtained by invoking a transform function on each element of the input sequence. If an OverflowException occurs, the Int32.MaxValue is returned.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The sum of the projected values.</returns>
        public static int SafeSum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            checked
            {
                try
                {
                    return source.Sum(selector);
                }
                catch (OverflowException)
                {
                    return int.MaxValue;
                }
            }
        }

        /// <summary>
        /// Computes the sum of the sequence of System.Int32 values that are obtained by invoking a transform function on each element of the input sequence. If an OverflowException occurs, the Int32.MaxValue is returned.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The sum of the projected values.</returns>
        public static int? SafeSum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            checked
            {
                try
                {
                    return source.Sum(selector);
                }
                catch (OverflowException)
                {
                    return int.MaxValue;
                }
            }
        }

        /// <summary>
        /// Computes the sum of the sequence of System.Int64 values that are obtained by invoking a transform function on each element of the input sequence. If an OverflowException occurs, the Int64.MaxValue is returned.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The sum of the projected values.</returns>
        public static long SafeSum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            checked
            {
                try
                {
                    return source.Sum(selector);
                }
                catch (OverflowException)
                {
                    return long.MaxValue;
                }
            }
        }

        /// <summary>
        /// Computes the sum of the sequence of System.Int64 values that are obtained by invoking a transform function on each element of the input sequence. If an OverflowException occurs, the Int64.MaxValue is returned.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The sum of the projected values.</returns>
        public static long? SafeSum<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            checked
            {
                try
                {
                    return source.Sum(selector);
                }
                catch (OverflowException)
                {
                    return long.MaxValue;
                }
            }
        }

        /// <summary>
        /// Computes the sum of the sequence of decimal values that are obtained by invoking a transform function on each element of the input sequence. If an OverflowException occurs, the decimal.MaxValue is returned.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The sum of the projected values.</returns>
        public static decimal SafeSum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            checked
            {
                try
                {
                    return source.Sum(selector);
                }
                catch (OverflowException)
                {
                    return decimal.MaxValue;
                }
            }
        }

        /// <summary>
        /// Computes the sum of the sequence of decimal values that are obtained by invoking a transform function on each element of the input sequence. If an OverflowException occurs, the decimal is returned.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The sum of the projected values.</returns>
        public static decimal? SafeSum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            checked
            {
                try
                {
                    return source.Sum(selector);
                }
                catch (OverflowException)
                {
                    return decimal.MaxValue;
                }
            }
        }

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
