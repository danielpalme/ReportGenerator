using Microsoft.Extensions.Configuration;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="Microsoft.Extensions.Configuration.IConfigurationBuilder"/> to add
    /// configuration from DotNetConfig sources.
    /// </summary>
    /// <remarks>Registers <see cref="DotNetConfigConfigurationSource"/> with the configuration
    /// builder.</remarks>
    public static class DotNetConfigConfigurationExtensions
    {
        /// <summary>
        /// Adds a DotNetConfig configuration source to the configuration builder.
        /// </summary>
        /// <param name="builder">The configuration builder to add the source to.</param>
        /// <returns>The same configuration builder instance.</returns>
        public static IConfigurationBuilder AddDotNetConfig(this IConfigurationBuilder builder) =>
            builder.Add(new DotNetConfigConfigurationSource());
    }
}