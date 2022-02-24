namespace Palmmedia.ReportGenerator.Core.Licensing
{
    /// <summary>
    /// Wrapps the license together with the signature.
    /// </summary>
    internal class LicenseWrapper
    {
        /// <summary>
        /// Gets or sets the license.
        /// </summary>
        public License License { get; set; }

        /// <summary>
        /// Gets or sets the signature.
        /// </summary>
        public string Signature { get; set; }
    }
}
