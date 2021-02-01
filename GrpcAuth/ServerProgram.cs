namespace GrpcAuth
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Https;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    public class ServerProgram
    {
        public static void Main(string[] args)
        {
            if (args.Contains("-h"))
            {
                Usage();
            }

            CreateHostBuilder(args).Build().Run();
        }

        private static void Usage()
        {
            Console.WriteLine(@"
Tool to test certificate authentication in Grpc

Usage:
    GrpcAuth [-bad]

Description:
    Starts a service which can be connected to using the client program. The
    server has its own private trust store and should only accept connection
    requests whose certificate chain is comprised of certs from the store.

Options:

-badname    The server will respond to connection requests with a name that
            does not match the address. The client should reject such requests
-badchain   The server will respond to connection requests with a certificate
            which is signed by an untrusted CA. The client should reject such
            requests.
");

            Environment.Exit(1);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ConfigureHttpsDefaults(listenOptions =>
                        {
                            listenOptions.ServerCertificate = GetServerCert(args);
                            listenOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                        });
                    });
                });

        private static X509Certificate2 GetServerCert(string[] args)
        {
            var certPath = @"ServerCerts\localhost.pfx";

            if (args.Contains("-badname"))
            {
                certPath = @"ServerCerts\badName.pfx";
            }
            else if (args.Contains("-badchain"))
            {
                certPath = @"ServerCerts\badChain.pfx";
            }

            return new X509Certificate2(certPath);
        }
    }
}