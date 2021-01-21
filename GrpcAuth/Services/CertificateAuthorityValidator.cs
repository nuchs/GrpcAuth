namespace GrpcAuth.Services
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Security.Cryptography.X509Certificates;

    public class CertificateAuthorityValidator : ICertificateAuthorityValidator
    {
        private readonly ILogger<CertificateAuthorityValidator> log;

        public CertificateAuthorityValidator(ILogger<CertificateAuthorityValidator> log)
            => this.log = log ?? throw new NullReferenceException(nameof(log));

        public bool IsValid(X509Certificate2 clientCert)
        {
            log.LogInformation($"Validating certificate within the {nameof(CertificateAuthorityValidator)}");
            return true;
        }
    }
}