namespace Palmmedia.ReportGenerator.Core.Licensing
{
    /// <summary>
    /// Specifies the cryptographic hash algorithm used for generating digital signatures.
    /// </summary>
    public static class SignatureType
    {
        /// <summary>
        /// Represents the SHA-1 cryptographic hash algorithm.
        /// </summary>
        public const string Sha1 = "Sha1";

        /// <summary>
        /// Represents the SHA-256 cryptographic hash algorithm.
        /// </summary>
        public const string Sha256 = "Sha256";
    }
}