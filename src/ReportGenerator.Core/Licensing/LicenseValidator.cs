using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Licensing
{
    internal static class LicenseValidator
    {
        private const string PublicRsaKey = "<RSAKeyValue><Modulus>vt+tb/fQ3r+7Rglb6c3n2pbkXNiNOTp85lFu8unk5MkFAlr5oYf6ADA6hVQvKadHBrFU22yGdddTqrEc4KkujMCr1XZ4bQ6phtPcVCXwEhjsurggPZtWUW7gi3FcRnksJfnIzVSsUFdA+3s0EXTwFuZFTQuUy8uPKsOaNST4InC5E04xY73dKs7++QoAg9grNdKJHMEp69JJ0jTiGzOmhh3ZoSmbDEywJnEP8Z14VrtAkJtzCSU4uiAhxmL1tUDanNJrET4DKkyXcPeroiVbdD8D7c4peWZTuq8mjtd8gyWz6WykIOvB1nst8hNG0Gn1sS9FioRaDFvFIBS9X2dsBQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(LicenseValidator));

        private static string cachedLicenseAsBase64;

        private static LicenseWrapper cachedLicense;

        private static bool exceptionLogged = false;

        public static bool IsValid(this string license)
        {
            if (string.IsNullOrEmpty(license))
            {
                return false;
            }

            if (cachedLicenseAsBase64 != license)
            {
                try
                {
                    cachedLicense = JsonSerializer.Deserialize<LicenseWrapper>(Convert.FromBase64String(license));
                    cachedLicenseAsBase64 = license;
                }
                catch (Exception ex)
                {
                    if (!exceptionLogged)
                    {
                        Logger.ErrorFormat(Resources.ErrorDuringDeserializingLicense, ex.Message);
                        exceptionLogged = true;
                    }

                    return false;
                }
            }

            try
            {
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    try
                    {
                        rsa.FromXmlString(PublicRsaKey);

                        return rsa.VerifyData(
                            Encoding.UTF8.GetBytes(cachedLicense.License.GetSignatureInput()),
                            SHA1.Create(),
                            Convert.FromBase64String(cachedLicense.Signature));
                    }
                    finally
                    {
                        rsa.PersistKeyInCsp = false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!exceptionLogged)
                {
                    Logger.ErrorFormat(Resources.ErrorDuringDeserializingLicense, ex.Message);
                    exceptionLogged = true;
                }

                return false;
            }
        }

        public static LicenseType DetermineLicenseType(this string license)
        {
            if (string.IsNullOrEmpty(license))
            {
                return LicenseType.None;
            }

            if (IsValid(license))
            {
                return LicenseType.Pro;
            }
            else
            {
                return LicenseType.None;
            }
        }
    }
}
