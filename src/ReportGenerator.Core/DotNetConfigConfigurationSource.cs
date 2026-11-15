using Microsoft.Extensions.Configuration;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Represents a configuration source that creates a DotNetConfigConfigurationProvider.
    /// </summary>
    /// <remarks>Used by the configuration builder to add DotNetConfig settings to the configuration
    /// pipeline.</remarks>
    public sealed class DotNetConfigConfigurationSource : IConfigurationSource
    {
        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            new DotNetConfigConfigurationProvider();
    }
}