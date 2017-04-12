using System;
using System.IO;
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

            var packet = new byte[16 + interfaceNameBytes.Length + methodNameBytes.Length + argBytes.Length];
		    var position = 0;
            Buffer.BlockCopy(preamble, 0, packet, 0, 4); // Preamble.
		    position += preamble.Length;
            
            Buffer.BlockCopy(interfaceNameBytesSize, 0, packet, position, 4); // Header interface name length.
            position += interfaceNameBytesSize.Length;

            Buffer.BlockCopy(methodNameBytesSize, 0, packet, position, 4); // Header method name length.
            position += methodNameBytesSize.Length;

            Buffer.BlockCopy(argBytesSize, 0, packet, position, 4); // Header arg lenght.
            position += argBytesSize.Length;

            Buffer.BlockCopy(interfaceNameBytes, 0, packet, position, interfaceNameBytes.Length);
            position += interfaceNameBytes.Length;

            Buffer.BlockCopy(methodNameBytes, 0, packet, position, methodNameBytes.Length);
            position += methodNameBytes.Length;

            Buffer.BlockCopy(argBytes, 0, packet, position, argBytes.Length);

		    return packet;
		}

		public static byte[] GeneratePacket(Packet packet)
		{
			return GeneratePacket(packet.InterfaceName, packet.MethodName, packet.Argument, packet.RequestHash);
		}
	}
}
