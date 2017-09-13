using System;
using Define.Classes.BaseArgs;
using ProtoBuf;

namespace Define.Classes.Args
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class GetServiceStatusResponse : Response
	{
	    public bool IsOnLine;
	}

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SetServiceStatusResponse : Response
	{
	    public int ClientHash;
	}

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class Pong : Response
    {
        public DateTime ReceivedTimeStamp;

	    public override string ToString()
	    {
		    return $"Pong => {this.ReceivedTimeStamp}";
	    }
    }
}
