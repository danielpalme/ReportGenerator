using System;
using System.Collections.Generic;
using System.Linq;

namespace Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch
{
    /// <summary>
    /// Searches several directories for class files.
    /// </summary>
    internal class MultiDirectoryClassSearcher : ClassSearcher
    {
        /// <summary>
        /// The <see cref="ClassSearcher">ClassSearchers</see>.
        /// </summary>
        private readonly IEnumerable<ClassSearcher> classSearchers;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiDirectoryClassSearcher"/> class.
        /// </summary>
        /// <param name="classSearchers">The <see cref="ClassSearcher">ClassSearchers</see>.</param>
        internal MultiDirectoryClassSearcher(IEnumerable<ClassSearcher> classSearchers)
        {
            if (classSearchers == null)
            {
                throw new ArgumentNullException(nameof(classSearchers));
            }

            this.classSearchers = classSearchers;
        }

        /// <summary>
        /// Gets the files the given class is defined in.
        /// </summary>
        /// <param name="className">Name of the class (with full namespace).</param>
        /// <returns>The files the class is defined in.</returns>
        internal override IEnumerable<string> GetFilesOfClass(string className) => this.classSearchers
                .SelectMany(c => c.GetFilesOfClass(className))
                .Distinct();
    }
}