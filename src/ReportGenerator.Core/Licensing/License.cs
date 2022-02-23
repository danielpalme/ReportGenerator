using System;

namespace Palmmedia.ReportGenerator.Core.Licensing
{
    internal class License
    {
        public Guid Id { get; set; }

        public string Login { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string LicenseType { get; set; }

        public DateTime IssuedAt { get; set; }

        public string GetSignatureInput()
        {
            return $"{this.Id:N}{this.Login}{this.Name}{this.Email}{this.LicenseType}{this.IssuedAt:yyyyMMddHH:mm:ss}";
        }
    }
}
