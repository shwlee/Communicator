using Define.Classes.BaseArgs;
using ProtoBuf;

namespace Define.Classes.Args
{
	[ProtoContract]
	public class GetServiceStatusRequest : Request
	{
	}

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SetServiceStatusRequest : Request
	{
	    public bool Status;
	}

    [ProtoContract]
    public class Ping : Request
    {
    }
}
