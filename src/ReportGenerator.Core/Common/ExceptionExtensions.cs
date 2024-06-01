using System;
using System.Reflection;
using System.Text;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Common
{
    /// <summary>
    /// Exception extensions.
    /// </summary>
    internal static class ExceptionExtensions
    {
        /// <summary>
        /// Gets a full error message especially for <see cref="AggregateException"/>.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>The error message.</returns>
        public static string GetExceptionMessageForDisplay(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            string message = exception.Message;

            if (exception is AggregateException aggregate)
            {
                var flat = aggregate.Flatten();

                if (flat.InnerExceptions.Count == 1)
                {
                    // recalling as we potentially can have a targetinvocationexception
                    message = GetExceptionMessageForDisplay(flat.InnerException);
                }
                else if (flat.InnerExceptions.Count > 0)
                {
                    var bldr = new StringBuilder();

                    bldr.AppendLine(Resources.MultipleErrors);

                    foreach (var innerEx in flat.InnerExceptions)
                    {
                        bldr.AppendLine(GetExceptionMessageForDisplay(innerEx));
                    }

                    message = bldr.ToString();
                }
            }
            else
            {
                if (exception is TargetInvocationException targetInvocationEx && targetInvocationEx.InnerException != null)
                {
                    message = GetExceptionMessageForDisplay(targetInvocationEx.InnerException);
                }
            }

            return message;
        }
    }
}
