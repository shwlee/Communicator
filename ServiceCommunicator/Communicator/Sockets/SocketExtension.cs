using System.Net.Sockets;
using System.Threading.Tasks;

namespace Communication.Sockets
{
	public static class SocketExtension
	{
		public static async Task<int> SendTaskAsync(this Socket socket, byte[] packet)
		{
			return await Task.Factory.FromAsync<int>(socket.BeginSend(packet, 0, packet.Length, SocketFlags.None, null, socket), socket.EndSend);			
		}
	}
}
