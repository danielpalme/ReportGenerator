using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Palmmedia.ReportGenerator.Core.Common
{
    /// <summary>
    /// Linq extensions.
    /// </summary>
    internal static class LinqExtensions
    {
        /// <summary>
        /// Creates a <see cref="HashSet&lt;T&gt;"/> from an <see cref="IEnumerable&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="input">The input.</param>
        /// <returns>A <see cref="HashSet&lt;T&gt;"/>.</returns>
        internal static HashSet<T> ToHashSet<T>(this IEnumerable<T> input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var result = new HashSet<T>();

            foreach (var item in input)
            {
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Determines whether a <see cref="XElement"/> has an <see cref="XAttribute"/> with the given value..
        /// </summary>
        /// <param name="element">The <see cref="XElement"/>.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns>
        ///   <c>true</c> if <see cref="XElement"/> has an <see cref="XAttribute"/> with the given value; otherwise, <c>false</c>.
        /// </returns>
        internal static bool HasAttributeWithValue(this XElement element, XName attributeName, string attributeValue)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            XAttribute attribute = element.Attribute(attributeName);

            if (attribute == null)
            {
                return false;
            }
            else
            {
                return string.Equals(attribute.Value, attributeValue, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Determines the distinct elements of a collection by the given key selector.
        /// </summary>
        /// <typeparam name="TSource">The type of source elements.</typeparam>
        /// <typeparam name="TKey">The type of the key elements.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns>The distinct elemtents.</returns>
        internal static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
