using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palmmedia.ReportGenerator.Reporting
{
    internal interface IClassFilter
    {
        /// <summary>
        /// Determines whether the given class should be included in the report.
        /// </summary>
        /// <param name="assemblyName">Name of the class.</param>
        /// <returns>
        ///   <c>true</c> if class should be included in the report; otherwise, <c>false</c>.
        /// </returns>
        bool IsClassIncludedInReport(string className);
    }
}
