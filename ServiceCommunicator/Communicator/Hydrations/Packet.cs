using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication.Hydrations
{
	public class Packet
	{
		public string InterfaceName { get; set; }

		public string MethodName { get; set; }

		public object Argument { get; set; }
	}
}
