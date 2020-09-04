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
            else if (obj is string text)
            {
                return $"\"{EscapeString(text)}\"";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{");

                int counter = 0;
                foreach (var property in obj.GetType().GetProperties())
                {
                    if (property.GetCustomAttributes(typeof(ObsoleteAttribute), false).Length > 0)
                    {
                        continue;
                    }

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

        /// <summary>
        /// Escapes string values.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The escaped string.</returns>
        public static string EscapeString(string input)
        {
            bool NeedEscape(string src, int i)
            {
                char c = src[i];
                return c < 32 || c == '"' || c == '\\'
                    || (c >= '\uD800' && c <= '\uDBFF' &&
                        (i == src.Length - 1 || src[i + 1] < '\uDC00' || src[i + 1] > '\uDFFF'))
                    || (c >= '\uDC00' && c <= '\uDFFF' &&
                        (i == 0 || src[i - 1] < '\uD800' || src[i - 1] > '\uDBFF'))
                    || c == '\u2028' || c == '\u2029'
                    || (c == '/' && i > 0 && src[i - 1] == '<');
            }

            var sb = new StringBuilder();

            int start = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (NeedEscape(input, i))
                {
                    sb.Append(input, start, i - start);
                    switch (input[i])
                    {
                        case '\b': sb.Append("\\b"); break;
                        case '\f': sb.Append("\\f"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        case '\"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '/': sb.Append("\\/"); break;
                        default:
                            sb.Append("\\u");
                            sb.Append(((int)input[i]).ToString("x04"));
                            break;
                    }

                    start = i + 1;
                }
            }

            sb.Append(input, start, input.Length - start);

            return sb.ToString();
        }
    }
}
