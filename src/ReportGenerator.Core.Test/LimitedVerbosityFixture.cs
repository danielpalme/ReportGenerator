using Palmmedia.ReportGenerator.Core.Logging;

namespace Palmmedia.ReportGenerator.Core.Test
{
    public class LimitedVerbosityFixture
    {
        public LimitedVerbosityFixture()
        {
            new ConsoleLoggerFactory().VerbosityLevel = VerbosityLevel.Warning;
        }
    }
}