using Define.Classes.Args;
using Define.Interfaces;
using System;
using System.Threading;

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
			//Thread.Sleep(2000);
			Console.WriteLine("[SetServiceStatus] @@@@@@@@@@ client hash : {0}", request.ClientHash);
			var clientHash = request.ClientHash;
            return new SetServiceStatusResponse { IsSuccess = true, ClientHash = ++clientHash };
        }

        public Pong KeepAlive(Ping request)
        {
            var sendTime = request.SendTimeStamp;
            Console.WriteLine("[KeepAlive] send time : {0}", sendTime);

			Thread.Sleep(2000);

			return new Pong
			{
				IsSuccess = true,
				ReceivedTimeStamp = DateTime.UtcNow
			};
		}
    }
}
