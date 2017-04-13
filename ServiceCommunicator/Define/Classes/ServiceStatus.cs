using System;
using System.Resources;
using Define.Classes.Args;
using Define.Interfaces;

namespace Define.Classes
{
    public class ServiceStatus : IServiceStatus
    {
        public GetServiceStatusResponse GetServiceStatus(GetServiceStatusRequest request)
        {
            Console.WriteLine("Received : GetServiceStatus!");
            return new GetServiceStatusResponse { IsOnLine = true, IsSuccess = true };
        }

        public SetServiceStatusResponse SetServiceStatus(SetServiceStatusRequest request)
        {
            //Console.WriteLine("[IServiceStatus SetServiceStatus] ClinetHash : {0}", request.ClientHash);
            var clientHash = request.ClientHash;
            return new SetServiceStatusResponse { IsSuccess = true, ClientHash = ++clientHash };
        }
    }
}
