using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Communication.Sockets
{
	public static class SocketExtension
	{
		public static async Task<int> SendTaskAsync(this Socket socket, byte[] packet)
		{
		    try
		    {
                Console.WriteLine("[Call SendTaskAsync] Packet Length : {0}", packet.Length);
                AsyncCallback nullOp = i => { };
                return await Task.Factory.FromAsync<int>(
                    // ReSharper disable once AssignNullToNotNullAttribute
                        socket.BeginSend(packet, 0, packet.Length, SocketFlags.None, nullOp, socket),
                        socket.EndSend);
		    }
		    catch (Exception ex)
		    {
		        Console.WriteLine(ex);
		        return 0;
		    }
		}
	}
}
