namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// Defines how CSS and JavaScript are referenced.
    /// </summary>
    internal enum HtmlMode
    {
        /// <summary>
        /// CSS and JavaScript is saved into separate files.
        /// </summary>
        ExternalCssAndJavaScript,

        /// <summary>
        /// CSS and JavaScript are saved into separate files but query string is appended dynamically to link 'href' and script 'src'.
        /// </summary>
        ExternalCssAndJavaScriptWithQueryStringHandling,

        /// <summary>
        /// CSS and JavaScript is included into the HTML instead of separate files.
        /// </summary>
        InlineCssAndJavaScript
    }
}
