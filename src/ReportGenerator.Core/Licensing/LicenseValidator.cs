using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Licensing
{
    /// <summary>
    /// Validates the user's license.
    /// </summary>
    internal static class LicenseValidator
    {
        /// <summary>
        /// The public RSA key.
        /// </summary>
        private const string PublicRsaKey = "<RSAKeyValue><Modulus>vt+tb/fQ3r+7Rglb6c3n2pbkXNiNOTp85lFu8unk5MkFAlr5oYf6ADA6hVQvKadHBrFU22yGdddTqrEc4KkujMCr1XZ4bQ6phtPcVCXwEhjsurggPZtWUW7gi3FcRnksJfnIzVSsUFdA+3s0EXTwFuZFTQuUy8uPKsOaNST4InC5E04xY73dKs7++QoAg9grNdKJHMEp69JJ0jTiGzOmhh3ZoSmbDEywJnEP8Z14VrtAkJtzCSU4uiAhxmL1tUDanNJrET4DKkyXcPeroiVbdD8D7c4peWZTuq8mjtd8gyWz6WykIOvB1nst8hNG0Gn1sS9FioRaDFvFIBS9X2dsBQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(LicenseValidator));

        /// <summary>
        /// Locked licenses.
        /// </summary>
        private static readonly Dictionary<Guid, DateTime> LockedLicences = new Dictionary<Guid, DateTime>()
        {
            { Guid.Parse("3aa02577-3966-4c4b-a24a-9c490b0e7a71"), DateTime.MinValue },
            { Guid.Parse("f7b8b8c5-811e-4b3d-92cf-f14a239ddca9"), DateTime.MinValue },
            { Guid.Parse("c05081c1-2ced-4bfd-8cb8-3cf23094369f"), DateTime.MinValue },
            { Guid.Parse("45404029-3119-4cc2-9e7c-aa9207673b15"), DateTime.MinValue },
            { Guid.Parse("8f316273-10a8-422f-992f-6c369cebdb92"), DateTime.MinValue },
            { Guid.Parse("0208bdf2-e775-4c5e-8765-4ebc615cbc17"), DateTime.MinValue },
            { Guid.Parse("860a488f-b3ce-4294-ae75-e331546ee830"), DateTime.MinValue },
            { Guid.Parse("8f66e7a8-e6c0-4a31-9747-67ed27b5f721"), DateTime.MinValue },
            { Guid.Parse("d78f42ba-a0b9-40b3-a9f7-b73bc31aa449"), DateTime.MinValue },
            { Guid.Parse("265472d9-799d-44db-b7f2-b8da433812f9"), new DateTime(2023, 7, 28) },
            { Guid.Parse("16823d5b-b7da-48b7-94d9-73dbce61059c"), new DateTime(2023, 11, 1) },
            { Guid.Parse("14978526-1719-48cb-aa6d-6d48b7a99af9"), new DateTime(2023, 11, 1) },
            { Guid.Parse("d15b4f19-506b-4c7c-a270-c99129bd8281"), new DateTime(2023, 12, 1) },
            { Guid.Parse("9717392c-fc55-415f-a1bf-1a407c9ec705"), new DateTime(2023, 12, 1) },
            { Guid.Parse("70dcfc78-6ca3-4a0b-bb43-a3840e39ff4f"), new DateTime(2024, 1, 1) },
            { Guid.Parse("b673472f-24e0-4b4b-9837-713d49049e2d"), new DateTime(2024, 2, 1) },
            { Guid.Parse("6e4d43dd-84a5-40fd-beb2-34f3c4930994"), new DateTime(2024, 4, 1) },
            { Guid.Parse("4b80b56a-1eb0-4b93-9b82-b33c11215316"), new DateTime(2024, 4, 1) },
            { Guid.Parse("1ee7e23a-a9ec-49cb-8c9c-f3c78d801f8d"), new DateTime(2024, 6, 1) },
            { Guid.Parse("f554292a-d126-402b-a7bf-615b35dc9cd2"), new DateTime(2024, 11, 1) },
            { Guid.Parse("2b1be47b-9785-4c22-a708-b58453e0a669"), new DateTime(2024, 11, 1) },
            { Guid.Parse("c2acc361-1b27-44a9-9e5d-841d8467dac0"), new DateTime(2025, 3, 1) },
        };

        /// <summary>
        /// The cached license in Base 64 format.
        /// </summary>
        private static string cachedLicenseAsBase64;

        /// <summary>
        /// The deserilized license.
        /// </summary>
        private static LicenseWrapper cachedLicense;

        /// <summary>
        /// Indicates whether an exception occured to prevent logging same exception several times.
        /// </summary>
        private static bool exceptionLogged = false;

        /// <summary>
        /// Validates the given license in Base 64 format.
        /// </summary>
        /// <param name="license">The license in Base 64 format containg a serialized <see cref="LicenseWrapper"/>.</param>
        /// <returns>True if license's signature is valid.</returns>
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

                        if (!rsa.VerifyData(
                            Encoding.UTF8.GetBytes(cachedLicense.License.GetSignatureInput()),
                            SHA1.Create(),
                            Convert.FromBase64String(cachedLicense.Signature)))
                        {
                            return false;
                        }

                        if (LockedLicences.TryGetValue(cachedLicense.License.Id, out DateTime lockDate))
                        {
                            if (DateTime.Today >= lockDate)
                            {
                                return false;
                            }
                        }

                        if (cachedLicense.License.LicenseType == "Pro")
                        {
                            return true;
                        }
                        else if (cachedLicense.License.LicenseType == "Trial"
                            && cachedLicense.License.ExpiresAt.HasValue)
                        {
                            return DateTime.UtcNow < cachedLicense.License.ExpiresAt.Value;
                        }

                        return false;
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

        /// <summary>
        /// Validates the license and determines the license type.
        /// </summary>
        /// <param name="license">The license in Base 64 format containg a serialized <see cref="LicenseWrapper"/>.</param>
        /// <returns>The license type.</returns>
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
