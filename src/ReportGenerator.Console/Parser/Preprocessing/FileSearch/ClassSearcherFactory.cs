using System;
using System.Collections.Generic;
using System.Linq;

namespace Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch
{
    /// <summary>
    /// Factory which provides <see cref="ClassSearcher"/> instances.
    /// Instances are cached and reused if the directory of a <see cref="ClassSearcher"/> starts with the desired path.
    /// This avoids scanning the same directory several times.
    /// </summary>
    internal class ClassSearcherFactory
    {
        /// <summary>
        /// The cached <see cref="ClassSearcher">ClassSearchers</see>.
        /// </summary>
        private readonly List<ClassSearcher> cachedClassSearchers = new List<ClassSearcher>();

        /// <summary>
        /// Creates the class searcher.
        /// </summary>
        /// <param name="directory">The directory that should be searched for class files.</param>
        /// <returns>The class searcher.</returns>
        internal ClassSearcher CreateClassSearcher(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return new ClassSearcher(null);
            }

            var cachedClassSearcher = this.cachedClassSearchers.FirstOrDefault(c => c.Directory != null && directory.StartsWith(c.Directory, StringComparison.OrdinalIgnoreCase));

            if (cachedClassSearcher == null)
            {
                cachedClassSearcher = new ClassSearcher(directory);
                this.cachedClassSearchers.Add(cachedClassSearcher);
            }

            return cachedClassSearcher;
        }

        /// <summary>
        /// Creates the class searcher.
        /// </summary>
        /// <param name="directories">The directories that should be searched for class files.</param>
        /// <returns>The class searcher.</returns>
        internal ClassSearcher CreateClassSearcher(params string[] directories)
        {
            var classSearchers = directories.Select(d => this.CreateClassSearcher(d)).ToArray();
            return new MultiDirectoryClassSearcher(classSearchers);
        }
    }
}