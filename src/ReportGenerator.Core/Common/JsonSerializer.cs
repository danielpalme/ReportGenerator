using System;
using System.Globalization;
using System.Text;

namespace Palmmedia.ReportGenerator.Core.Common
{
    /// <summary>
    /// Simple JSON serializer.
    /// </summary>
    public class JsonSerializer
    {
        /// <summary>
        /// Converts the given object to JSON.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>The JSON string.</returns>
        public static string ToJsonString(object obj)
        {
            if (obj == null)
            {
                return "null";
            }

            if (obj.GetType().IsValueType)
            {
                if (obj is bool)
                {
                    return obj.ToString().ToLowerInvariant();
                }
                else
                {
                    return Convert.ToString(obj, CultureInfo.InvariantCulture);
                }
            }
            else if (obj is string)
            {
                return $"\"{obj}\"";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{");

                int counter = 0;
                foreach (var property in obj.GetType().GetProperties())
                {
                    if (counter > 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append(" \"");
                    sb.Append(property.Name);
                    sb.Append("\": ");
                    sb.Append(ToJsonString(property.GetValue(obj)));

                    counter++;
                }

                sb.Append(" }");

                return sb.ToString();
            }
        }
    }
}
