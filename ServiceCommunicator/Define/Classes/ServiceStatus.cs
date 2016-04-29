using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Define.Classes.Args;
using Define.Interfaces;

namespace Define.Classes
{
	public class ServiceStatus : IServiceStatus
	{
		public GetServiceStatusResponse GetServiceStatus(GetServiceStatusRequest request)
		{
			return new GetServiceStatusResponse { IsOnLine = true, IsSuccess = true };			
		}

		public SetServiceStatusResponse SetServiceStatus(SetServiceStatusRequest request)
		{
			return new SetServiceStatusResponse { IsSuccess = true };
		}
	}
}
