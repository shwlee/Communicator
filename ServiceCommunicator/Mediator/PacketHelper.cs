using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediator
{
	public class PacketHelper
	{
		// preamble : request hash (4),
		// header : interface name size (4) + method name size (4) + arg size (4),
		// body : interface name + method name + serialized arg.

		private const int preambleSize = 4;
		private const int headerSize = 4;
		

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

		public static T ParsePacket<T>(byte[] packet, Type argType)
			where T : class
		{
			using (var memoryStream = new MemoryStream(packet))
			{
				using (var binaryReader = new BinaryReader(memoryStream))
				{
					var preambleBytes =  binaryReader.ReadBytes(4);
					var preamble = BitConverter.ToInt32(preambleBytes, 0);

					var interfaceNameSizeBytes = binaryReader.ReadBytes(4);
					var interfaceNameSize = BitConverter.ToInt32(interfaceNameSizeBytes, 0);

					var methodNameSizeBytes = binaryReader.ReadBytes(4);
					var methodNameSize = BitConverter.ToInt32(methodNameSizeBytes, 0);

					var argSizeBytes = binaryReader.ReadBytes(4);
					var argSize = BitConverter.ToInt32(argSizeBytes, 0);

					var interfaceNameBytes = binaryReader.ReadBytes(interfaceNameSize);
					var interfaceName = Encoding.UTF8.GetString(interfaceNameBytes);

					var methodNameBytes = binaryReader.ReadBytes(methodNameSize);
					var methodName = Encoding.UTF8.GetString(methodNameBytes);

					var argBytes = binaryReader.ReadBytes(argSize);
					using (var argStream = new MemoryStream(argBytes))
					{
						var arg = ProtoBuf.Serializer.NonGeneric.Deserialize(argType, argStream);
						return arg as T;
					}
				}
			}			
		}

		public static T ParseArgument<T>(byte[] packet)
			where T : class
		{
			using (var memoryStream = new MemoryStream(packet))
			{
				using (var binaryReader = new BinaryReader(memoryStream))
				{
					var preambleBytes = binaryReader.ReadBytes(4);
					var preamble = BitConverter.ToInt32(preambleBytes, 0);

					var interfaceNameSizeBytes = binaryReader.ReadBytes(4);
					var interfaceNameSize = BitConverter.ToInt32(interfaceNameSizeBytes, 0);

					var methodNameSizeBytes = binaryReader.ReadBytes(4);
					var methodNameSize = BitConverter.ToInt32(methodNameSizeBytes, 0);

					var argSizeBytes = binaryReader.ReadBytes(4);
					var argSize = BitConverter.ToInt32(argSizeBytes, 0);

					var interfaceNameBytes = binaryReader.ReadBytes(interfaceNameSize);
					var interfaceName = Encoding.UTF8.GetString(interfaceNameBytes);

					var methodNameBytes = binaryReader.ReadBytes(methodNameSize);
					var methodName = Encoding.UTF8.GetString(methodNameBytes);

					var argBytes = binaryReader.ReadBytes(argSize);
					using (var argStream = new MemoryStream(argBytes))
					{
						var arg = ProtoBuf.Serializer.Deserialize<T>(argStream);
						return arg;
					}
				}
			}
		}
	}
}
