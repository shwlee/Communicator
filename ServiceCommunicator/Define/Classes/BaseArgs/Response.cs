﻿using Define.Classes.Args;
using ProtoBuf;

namespace Define.Classes.BaseArgs
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, AsReferenceDefault = true)]
	[ProtoInclude(10000, typeof(GetServiceStatusResponse))]
	[ProtoInclude(10001, typeof(SetServiceStatusResponse))]
	public class Response
	{
	    public bool IsSuccess;
	}
}
