using Grpc.Net.Client;
using GrpcGreeterClient;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Client
{
    internal class ClientProgram
    {
        private static async Task Main(string[] args)
        {
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GetChannel(args);
            var client = new Greeter.GreeterClient(channel);

            var reply = await client.SayHelloAsync(
                              new HelloRequest { Name = "GreeterClient" });
            Console.WriteLine("Greeting: " + reply.Message);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static GrpcChannel GetChannel(string[] args)
        {
            if (args.Contains("-trust"))
            {
                // Signed by the correct CA
                return CreateSecureChannel("trustedClient.pfx");
            }
            else if (args.Contains("-untrust"))
            {
                // Signed by an untrusted CA
                return CreateSecureChannel("untrustedClient.pfx");
            }
            else
            {
                // Don't use a cert
                return GrpcChannel.ForAddress("https://localhost:5001");
            }
        }

        private static GrpcChannel CreateSecureChannel(string certName)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(new X509Certificate2(@$"Certs\{certName}"));
            return GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions { HttpHandler = handler });
        }
    }
}