using Define.Classes.Args;
using ProtoBuf;

namespace Define.Classes.BaseArgs
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	[ProtoInclude(20000, typeof(GetServiceStatusRequest))]
	[ProtoInclude(20001,  typeof(SetServiceStatusRequest))]
    [ProtoInclude(20002, typeof(Ping))]
	public class Request
	{
	    public int ClientHash;
	}
}
