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
                Console.WriteLine("Trust chain : client -> trusted intermediate -> trusted CA");
                return CreateSecureChannel("appTrustChainClient.pfx");
            }
            else if (args.Contains("-root"))
            {
                Console.WriteLine("Trust chain : client -> host (not server) trusts intermediate -> trusted CA");
                return CreateSecureChannel("appTrustRootClient.pfx");
            }
            else if (args.Contains("-int"))
            {
                Console.WriteLine("Trust chain : client -> trusted intermediate -> host (not server) trusts CA");
                return CreateSecureChannel("appTrustIntClient.pfx");
            }
            else if (args.Contains("-host"))
            {
                Console.WriteLine("Trust chain : client -> host (not server) trusts intermediate -> host (not server) trusts CA");
                return CreateSecureChannel("hostTrustClient.pfx");
            }
            else if (args.Contains("-untrust"))
            {
                Console.WriteLine("Trust chain : client -> untrusted intermediate -> untrusted CA");
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