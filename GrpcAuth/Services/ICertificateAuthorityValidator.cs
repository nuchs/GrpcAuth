namespace GrpcAuth.Services
{
    using System.Security.Cryptography.X509Certificates;

    internal interface ICertificateAuthorityValidator
    {
        bool IsValid(X509Certificate2 clientCert);
    }
}