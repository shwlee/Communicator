using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Define.Classes.BaseArgs
{
	[ProtoContract(ImplicitFields =ImplicitFields.AllPublic)]
	[ProtoInclude(10000, "GetServiceStatusResponse")]
	[ProtoInclude(10001, "SetServiceStatusResponse")]
	public class Response
	{
		public bool IsSuccess { get; set; }
	}
}
