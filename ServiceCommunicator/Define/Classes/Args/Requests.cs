using Define.Classes.BaseArgs;

namespace Define.Classes.Args
{
	public class GetServiceStatusRequest : Request
	{
	}

	public class SetServiceStatusRequest : Request
	{
		public bool Status { get; set; }
	}
}
