using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Communication.Core.Sockets
{
	public static class SocketExtension
	{
		public static async Task ConnectToAsync(this Socket socket, string ip, int port)
		{
			try
			{
				var host = IPAddress.Parse(ip);
				
				Console.WriteLine($"[Call ConnectToAsync] Packet Length = {ip} : {port}");
				
				await Task.Factory.FromAsync(
						// ReSharper disable once AssignNullToNotNullAttribute
						socket.BeginConnect,
						socket.EndConnect,
						host,
						port,
						null);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}

		public static async Task<int> SendPacketAsync(this Socket socket, byte[] packet)
		{
		    try
		    {
                Console.WriteLine("[Call SendTaskAsync] Packet Length : {0}", packet.Length);
                AsyncCallback nullOp = i => { };
                return await Task.Factory.FromAsync(
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
