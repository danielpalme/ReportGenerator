using System;
using System.Reflection;

namespace Palmmedia.ReportGenerator.Core.Plugin
{
    /// <summary>
    /// This wrapper was introduced as a workaround in .NET Core SDK 3.1.20x.
    /// Casting types from external DLLs fails within MSBuild.
    /// See:
    /// https://github.com/danielpalme/ReportGenerator/issues/335.
    /// https://github.com/dotnet/sdk/issues/11043.
    /// </summary>
    public class ReflectionWrapperAssemblyLoader : IAssemblyLoader
    {
        /// <summary>
        /// The <see cref="IAssemblyLoader"/> to wrap.
        /// </summary>
        private readonly object assemblyLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionWrapperAssemblyLoader" /> class.
        /// </summary>
        /// <param name="assemblyLoader">The <see cref="IAssemblyLoader"/> to wrap.</param>
        public ReflectionWrapperAssemblyLoader(object assemblyLoader)
        {
            this.assemblyLoader = assemblyLoader ?? throw new ArgumentNullException(nameof(assemblyLoader));
        }

        /// <summary>
        /// Loads the assembly with the given name.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <returns>The assembly.</returns>
        public Assembly Load(string name)
        {
            var assembly = this.assemblyLoader.GetType()
                .GetMethod(nameof(this.Load))
                .Invoke(this.assemblyLoader, new[] { name });

            return (Assembly)assembly;
        }
    }
}
