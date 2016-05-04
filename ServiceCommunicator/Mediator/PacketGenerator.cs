using System;
using System.IO;
using System.Linq;
using System.Text;
using Common.Communication;

namespace Mediator
{
	public class PacketGenerator
	{
		// preamble : request hash (4),
		// header : interface name size (4) + method name size (4) + arg size (4),
		// body : interface name + method name + serialized arg.
		
		public static byte[] GeneratePacket(string interfaceName, string methodName, object arg, int requestHash)
		{
			var preamble = BitConverter.GetBytes(requestHash);
			var interfaceNameBytes = Encoding.UTF8.GetBytes(interfaceName);
			var methodNameBytes = Encoding.UTF8.GetBytes(methodName);

			byte[] argBytes;
			using(var ms = new MemoryStream())
			{
				ProtoBuf.Serializer.NonGeneric.Serialize(ms, arg);
				argBytes = ms.ToArray();
			}

			var interfaceNameBytesSize = BitConverter.GetBytes(interfaceNameBytes.Length);
			var methodNameBytesSize = BitConverter.GetBytes(methodNameBytes.Length);
			var argBytesSize = BitConverter.GetBytes(argBytes.Length);
			var packet = preamble. // Preamble
				Concat(interfaceNameBytesSize). // Header
				Concat(methodNameBytesSize). // Header
				Concat(argBytesSize).  // Header
				Concat(interfaceNameBytes).Concat(methodNameBytes).Concat(argBytes); // Body
						
			return packet.ToArray();
		}

		public static byte[] GeneratePacket(Packet packet)
		{
			return GeneratePacket(packet.InterfaceName, packet.MethodName, packet.Argument, packet.RequestHash);
		}
	}
}
