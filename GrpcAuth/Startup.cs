namespace GrpcAuth
{
    using Microsoft.AspNetCore.Authentication.Certificate;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System.Security.Cryptography.X509Certificates;

    public class Startup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization(); // UseAuthentication only tells you if a user is authenticated or not, it doesn't take any actions. Use authorization is required to actually block bad users
            app.UseEndpoints(endpoints =>
            {
                endpoints
                    .MapGrpcService<GreeterService>()
                    .RequireAuthorization(); // The default authorization policy is allow any old bugger, this flips it so all grpc routes only allow authorized users

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client.");
                });
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate(options =>
                {
                    options.CustomTrustStore = GetTrustedCerts(); // Use these certs to establish a chain of trust rather than the system store
                    options.ChainTrustValidationMode = X509ChainTrustMode.CustomRootTrust; // This needs to be set otherwise options.CustomTrustStore is ignored
                    options.RevocationMode = X509RevocationMode.NoCheck;
                    options.AllowedCertificateTypes = CertificateTypes.Chained; // Self signed certs are auto rejected
                    options.ValidateCertificateUse = true; // Checks that it has the correct extensions for a client cert
                    options.ValidateValidityPeriod = true;
                });

            services.AddAuthorization();
            services.AddGrpc();
        }

        private X509Certificate2Collection GetTrustedCerts()
        {
            using var rootStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            rootStore.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            var trustedCerts = rootStore.Certificates.Find(X509FindType.FindByThumbprint, "54baea1c838d09981a71410637ebbd2cc34b3367", validOnly: true);
            return trustedCerts;
        }
    }
}