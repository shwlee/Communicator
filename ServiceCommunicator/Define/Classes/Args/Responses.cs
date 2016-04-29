using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Define.Classes.BaseArgs;

namespace Define.Classes.Args
{
	public class GetServiceStatusResponse : Response
	{
		public bool IsOnLine { get; set; }
	}

	public class SetServiceStatusResponse : Response
	{		
	}
}
