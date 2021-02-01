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

        public static async Task Main(string[] args)
        {
            if (args.Contains("-h"))
            {
                Usage();
            }

            using var channel = GetChannel(args);
            var client = new Greeter.GreeterClient(channel);

            var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });

            Console.WriteLine("Greeting: " + reply.Message);
        }

        private static void Usage()
        {
            Console.WriteLine(@"
Tool to test certificate authentication in Grpc

Usage:
    Client.exe [option]

Setup:
    Before running this program you must have installed all of the certificates
    in the ""Install These"" folder. The ca_* certs should go in as Trust Root
    Certificate Authorities and the int_* certs should go in as Intermediate
    Certificate Authorities. You must also have the GrpcAuth service running on
    localhost:5001.

Description
    The client will attempt to connect to the GrpcAuth service and send a simple
    ""Hello"" message, you can instruct it to connect with a variety of
    different certificates which are specified using a command line option. If
    no arguments are specified then the client will try to connect without a
    certificate.

    The client and server should fail to make a connection unless both sides
    present a certificate chain which is fully trusted by the other.

    If required you can regenerate all of the certificates using the certgen
    script (requires bash).

Options:

-trust      The GrpcAuth server fully trusts the certificate chain of the client
            cert. Connections using this cert should succeed
-root       The GrpcAuth server trusts the root of the certificate chain but not
            the intermediates. Connections using this certificate should fail
-int        The GrpcAuth server trusts the intermedoate certificates in this
            chain but not the root. Connections made using this certificate
            should fail.
-host       The host trusts the certificate in certificate chain but the
            GrpcAuth server does not. Connections made using this certificate
            should fail.
-untrust    Neither the host, nor the server trust any of the certs in this
            chain. Connections made using this certificate should fail.
");
            Environment.Exit(1);
        }

        private static GrpcChannel CreateSecureChannel(string certName)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(new X509Certificate2(@$"ClientCerts\{certName}"));
            return GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions { HttpHandler = handler });
        }

        private static GrpcChannel GetChannel(string[] args)
        {
            if (args.Contains("-trust"))
            {
                Console.WriteLine("Trust chain : client -> trusted intermediate -> trusted CA");
                return CreateSecureChannel("client_appTrusted.pfx");
            }
            else if (args.Contains("-root"))
            {
                Console.WriteLine("Trust chain : client -> host, not server, trusts intermediate -> trusted CA");
                return CreateSecureChannel("client_appTrustsRootNotInt.pfx");
            }
            else if (args.Contains("-int"))
            {
                Console.WriteLine("Trust chain : client -> trusted intermediate -> host, not server, trusts CA");
                return CreateSecureChannel("client_appTrustsIntNotRoot.pfx");
            }
            else if (args.Contains("-host"))
            {
                Console.WriteLine("Trust chain : client -> host, not server, trusts intermediate -> host, not server, trusts CA");
                return CreateSecureChannel("client_hostTrusted.pfx");
            }
            else if (args.Contains("-untrust"))
            {
                Console.WriteLine("Trust chain : client -> untrusted intermediate -> untrusted CA");
                return CreateSecureChannel("client_untrusted.pfx");
            }
            else
            {
                Console.WriteLine("Connecting to server without using a cert");
                return GrpcChannel.ForAddress(serverAddress);
            }
        }
    }
}