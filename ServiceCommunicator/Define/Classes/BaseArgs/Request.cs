using Define.Classes.Args;
using ProtoBuf;

namespace Define.Classes.BaseArgs
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	[ProtoInclude(20000, typeof(GetServiceStatusRequest))]
	[ProtoInclude(20001,  typeof(SetServiceStatusRequest))]
	public class Request
	{
	    public int ClientHash;
	}
}
