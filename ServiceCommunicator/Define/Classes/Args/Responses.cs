using Define.Classes.BaseArgs;
using ProtoBuf;

namespace Define.Classes.Args
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class GetServiceStatusResponse : Response
	{
		public bool IsOnLine { get; set; }
	}

	[ProtoContract()]
	public class SetServiceStatusResponse : Response
	{		
	}
}
