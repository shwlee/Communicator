using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Communication.Sockets
{
	public class StateObject
	{
		public Guid ClientId { get; set; }
		public Socket WorkSocket { get; set; }
		public const int BUFFER_SIZE = 2048;
		public byte[] buffer = new byte[BUFFER_SIZE];
	}
}
