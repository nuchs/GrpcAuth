namespace Client
{
    using Grpc.Net.Client;
    using GrpcGreeterClient;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    internal class ClientProgram
    {
        private const string serverAddress = "https://localhost:5001";

        private static async Task Main(string[] args)
        {
            using var channel = GetChannel(args);
            var client = new Greeter.GreeterClient(channel);

            var reply = await client.SayHelloAsync(
                              new HelloRequest { Name = "GreeterClient" });

            Console.WriteLine("Greeting: " + reply.Message);
        }

        private static GrpcChannel GetChannel(string[] args)
        {
            if (args.Contains("-trust"))
            {
                Console.WriteLine("Connecting to server using a cert signed by the correct trusted CA");
                return CreateSecureChannel("trustedClient.pfx");
            }
            else if (args.Contains("-other"))
            {
                Console.WriteLine("Connecting to server using a cert signed by the wrong trusted CA");
                return CreateSecureChannel("otherClient.pfx");
            }
            else if (args.Contains("-untrust"))
            {
                Console.WriteLine("Connecting to server using a cert signed by an untrusted CA");
                return CreateSecureChannel("untrustedClient.pfx");
            }
            else
            {
                Console.WriteLine("Connecting to server without using a cert");
                return GrpcChannel.ForAddress(serverAddress);
            }
        }

        private static GrpcChannel CreateSecureChannel(string certName)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(new X509Certificate2(@$"Certs\{certName}"));
            return GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions { HttpHandler = handler });
        }
    }
}