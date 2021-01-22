namespace GrpcAuth
{
    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;

        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            var callerId = ExtractCallerId(context);

            return Task.FromResult(new HelloReply
            {
                Message = $"Hello {request.Name} or should I call you {callerId}"
            });
        }

        private static string ExtractCallerId(ServerCallContext context)
        {
            var authContext = context.AuthContext;
            var idPropName = authContext.PeerIdentityPropertyName;

            if (authContext.IsPeerAuthenticated && idPropName is not null)
            {
                var idProp = authContext.FindPropertiesByName(idPropName).First();
                return idProp.Value;
            }
            else
            {
                throw new InvalidOperationException("Gerouttamapub");
            }
        }
    }
}