using System;
using System.Net.Sockets;

namespace Communication.Sockets
{
	public class StateObject
	{
		public Guid ClientId { get; set; }

		public Socket WorkSocket { get; set; }

		public const int BUFFER_SIZE = 2048;

		public byte[] Buffer = new byte[BUFFER_SIZE];

        public PacketHandler PacketHandler { get; set; }
	}
}
