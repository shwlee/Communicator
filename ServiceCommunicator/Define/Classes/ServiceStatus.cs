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
            Console.WriteLine("Received : SetServiceStatus!");
		    var isSuccess = false;
            if (request != null && request.Status)
            {
                isSuccess = request.ClientHash > 10;
            }

            return new SetServiceStatusResponse { IsSuccess = isSuccess };
		}
	}
}
