namespace GrpcAuth
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Https;
    using Microsoft.Extensions.Hosting;
    using System.Security.Cryptography.X509Certificates;

    public class ServerProgram
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ConfigureHttpsDefaults(listenOptions =>
                        {
                            listenOptions.ServerCertificate = new X509Certificate2(@"Certs\grpc-server.pfx");
                            listenOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                        });
                    });
                });
    }
}