namespace Palmmedia.ReportGenerator.Core.Reporting
{
    /// <summary>
    /// Interface indicating that an <see cref="IReportBuilder"/> can build multiple reports concurrently.
    /// </summary>
    public interface IParallelisableReportBuilder : IReportBuilder { }
}
