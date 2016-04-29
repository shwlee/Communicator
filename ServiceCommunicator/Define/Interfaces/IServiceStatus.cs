using Define.Classes.Args;
using Define.Classes.BaseArgs;

namespace Define.Interfaces
{
	public interface IServiceStatus
	{
		GetServiceStatusResponse GetServiceStatus(GetServiceStatusRequest request);

		SetServiceStatusResponse SetServiceStatus(SetServiceStatusRequest request);
	}
}
