using System;

namespace Palmmedia.ReportGenerator.Core.Licensing
{
    /// <summary>
    /// The license.
    /// </summary>
    internal class License
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the login.
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the license type.
        /// </summary>
        public string LicenseType { get; set; }

        /// <summary>
        /// Gets or sets the issue at date.
        /// </summary>
        public DateTime IssuedAt { get; set; }

        /// <summary>
        /// Gets or sets the issue at date.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Gets a string containing the relevant properties for the signature.
        /// </summary>
        /// <returns>The string containing the relevant properties for the signature.</returns>
        public string GetSignatureInput()
        {
            return $"{this.Id:N}{this.Login}{this.Name}{this.Email}{this.LicenseType}{this.IssuedAt:yyyyMMddHH:mm:ss}{this.ExpiresAt:yyyyMMddHH:mm:ss}";
        }
    }
}
