namespace Palmmedia.ReportGenerator
{
    /// <summary>
    /// Command line access to the ReportBuilder.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The main method.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Return code indicating success/failure.</returns>
        internal static int Main(string[] args)
        {
            return Core.Program.Main(args);
        }
    }
}
