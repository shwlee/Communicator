using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
