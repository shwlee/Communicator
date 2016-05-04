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
