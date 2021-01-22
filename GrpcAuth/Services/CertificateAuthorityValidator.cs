namespace GrpcAuth.Services
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Security.Cryptography.X509Certificates;

    public class CertificateAuthorityValidator : ICertificateAuthorityValidator
    {
        private readonly ILogger<CertificateAuthorityValidator> _logger;

        // this should probably be injected via config or loaded from the cert
        // Apparently the bytes are in the reverse order when using this BigInteger parse method,
        // hence the reverse
        private readonly byte[] _caCertSubjectKeyIdentifier = BigInteger.Parse(
            "e9be86f64eb53bc12c1b5fe0f63df450274811da",
            System.Globalization.NumberStyles.HexNumber
        ).ToByteArray().Reverse().ToArray();

        private const string AuthorityKeyIdentifier = "Authority Key Identifier";

        public CertificateAuthorityValidator(ILogger<CertificateAuthorityValidator> logger)
        {
            _logger = logger;
        }

        public bool IsValid(X509Certificate2 clientCert)
        {
            return true;
        }
    }
}