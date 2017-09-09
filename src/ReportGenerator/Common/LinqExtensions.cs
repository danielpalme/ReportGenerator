using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Palmmedia.ReportGenerator.Common
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
        /// Creates a <see cref="Queue&lt;T&gt;"/> from an <see cref="IEnumerable&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="input">The input.</param>
        /// <returns>A <see cref="Queue&lt;T&gt;"/>.</returns>
        internal static Queue<T> ToQueue<T>(this IEnumerable<T> input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var result = new Queue<T>();

            foreach (var item in input)
            {
                result.Enqueue(item);
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
    }
}
