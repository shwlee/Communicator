using System;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using Define.Classes.Args;
using Define.Classes.BaseArgs;
using Define.Interfaces;

namespace Define.Classes
{
    public class ServiceStatus : IServiceStatus
    {
        #region IServiceStatus Members

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

        public async Task<Pong> KeepAlive(Ping request)
        {
            var sendTime = request.SendTimeStamp;
            Console.WriteLine("[KeepAlive] send time : {0}", sendTime);
            return await Task.Run(() =>
            {
                Thread.Sleep(2000);

                return new Pong
                {
                    IsSuccess = true,
                    ReceivedTimeStamp = DateTime.UtcNow
                };
            });
        }

        #endregion
    }
}
