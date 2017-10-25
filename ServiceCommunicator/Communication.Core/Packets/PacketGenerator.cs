using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Communication.Common.Buffers;
using Communication.Common.Packets;
using Communication.Core.AsyncResponse;
using Communication.Core.Mediator;
using Communication.Core.Sockets;

namespace Communication.Core.Packets
{
	public class PacketGenerator
	{
		// sizeheader : whole packet size (4),
		// preamble : request hash (4),
		// header : interface name size (4) + method name size (4) + arg size (4),
		// body : interface name + method name + serialized arg.

		public const int HeaderSize = 20;

		public const int HeaderUnitSize = 4;
		
		public static byte[] GeneratePacket(string interfaceName, string methodName, object arg, int requestHash)
		{
			var preamble = BitConverter.GetBytes(requestHash);
			var interfaceNameBytes = Encoding.UTF8.GetBytes(interfaceName);
			var methodNameBytes = Encoding.UTF8.GetBytes(methodName);

			byte[] argBytes;
			using (var ms = new MemoryStream())
			{
				ProtoBuf.Serializer.NonGeneric.Serialize(ms, arg);
				argBytes = ms.ToArray();
			}

			var interfaceNameBytesSize = BitConverter.GetBytes(interfaceNameBytes.Length);
			var methodNameBytesSize = BitConverter.GetBytes(methodNameBytes.Length);
			var argBytesSize = BitConverter.GetBytes(argBytes.Length);

			var wholeSize = HeaderSize + interfaceNameBytes.Length + methodNameBytes.Length + argBytes.Length;
			var wholeSizeBytes = BitConverter.GetBytes(wholeSize);

			var packet = BufferPool.Instance.GetBuffer(wholeSize);
			
			var position = 0;
			Buffer.BlockCopy(wholeSizeBytes, 0, packet, 0, HeaderUnitSize); // whole size.
			position += wholeSizeBytes.Length;
			
			Buffer.BlockCopy(preamble, 0, packet, position, HeaderUnitSize); // Preamble.
			position += preamble.Length;

			Buffer.BlockCopy(interfaceNameBytesSize, 0, packet, position, HeaderUnitSize); // Header interface name length.
			position += interfaceNameBytesSize.Length;

			Buffer.BlockCopy(methodNameBytesSize, 0, packet, position, HeaderUnitSize); // Header method name length.
			position += methodNameBytesSize.Length;

			Buffer.BlockCopy(argBytesSize, 0, packet, position, HeaderUnitSize); // Header arg lenght.
			position += argBytesSize.Length;

			Buffer.BlockCopy(interfaceNameBytes, 0, packet, position, interfaceNameBytes.Length);
			position += interfaceNameBytes.Length;

			Buffer.BlockCopy(methodNameBytes, 0, packet, position, methodNameBytes.Length);
			position += methodNameBytes.Length;

			Buffer.BlockCopy(argBytes, 0, packet, position, argBytes.Length);

			return packet;
		}

		public static byte[] GeneratePacket(Message message)
		{
			return GeneratePacket(message.InterfaceName, message.MethodName, message.Argument, message.RequestHash);
		}

		public static int GetPreamble(byte[] packet)
		{
			var preambleBuffer = BufferPool.Instance.GetBuffer(HeaderUnitSize);

			for (var i = 0; i < HeaderUnitSize; i++)
			{
				preambleBuffer[i] = packet[i + 4]; // skip wholeSize
			}

			var preamble = BitConverter.ToInt32(preambleBuffer, 0);

			BufferPool.Instance.ReturnBuffer(preambleBuffer);

			return preamble;
		}

		/// <summary>
		/// Parse packet and execute message.
		/// </summary>
		/// <param name="sourceBuffer"></param>
		/// <param name="readPosition"></param>
		/// <param name="lastPosition"></param>
		/// <param name="headerBuffer"></param>
		/// <param name="mediator"></param>
		/// <param name="responseSocket"></param>
		/// <returns>if it is true, need receive more packets. </returns>
		public static bool ParseAndExecute(
			ref int readPosition, 
			byte[] sourceBuffer,
			int lastPosition,
			byte[] headerBuffer,
			InstanceMediator mediator,
			Socket responseSocket)
		{
			try
			{
				Array.Clear(headerBuffer, 0, HeaderUnitSize);

				var readStart = readPosition;

				ReadBuffer(sourceBuffer, headerBuffer, readPosition);
				var wholeSize = BitConverter.ToInt32(headerBuffer, 0);
				readPosition += HeaderUnitSize;

				if (wholeSize > lastPosition)
				{
					readPosition = readStart;
					return true;
				}

				ReadBuffer(sourceBuffer, headerBuffer, readPosition);
				var preamble = BitConverter.ToInt32(headerBuffer, 0);
				readPosition += HeaderUnitSize;

				ReadBuffer(sourceBuffer, headerBuffer, readPosition);
				var interfaceNameSize = BitConverter.ToInt32(headerBuffer, 0);
				readPosition += HeaderUnitSize;

				ReadBuffer(sourceBuffer, headerBuffer, readPosition);
				var methodNameSize = BitConverter.ToInt32(headerBuffer, 0);
				readPosition += HeaderUnitSize;

				ReadBuffer(sourceBuffer, headerBuffer, readPosition);
				var argSize = BitConverter.ToInt32(headerBuffer, 0);
				readPosition += HeaderUnitSize;

				var interfaceNameBuffer = BufferPool.Instance.GetBuffer(interfaceNameSize);
				ReadBuffer(sourceBuffer, interfaceNameBuffer, readPosition);
				var interfaceName = Encoding.UTF8.GetString(interfaceNameBuffer);
				readPosition += interfaceNameBuffer.Length;
				BufferPool.Instance.ReturnBuffer(interfaceNameBuffer);

				var methodNameBuffer = BufferPool.Instance.GetBuffer(methodNameSize);
				ReadBuffer(sourceBuffer, methodNameBuffer, readPosition);
				var methodName = Encoding.UTF8.GetString(methodNameBuffer);
				readPosition += methodNameBuffer.Length;
				BufferPool.Instance.ReturnBuffer(methodNameBuffer);

				var argsBuffer = BufferPool.Instance.GetBuffer(argSize);
				ReadBuffer(sourceBuffer, argsBuffer, readPosition);
				readPosition += argSize;

				var completedPacket = BufferPool.Instance.GetBuffer(wholeSize);
				Buffer.BlockCopy(sourceBuffer, readStart, completedPacket, 0, wholeSize);
				
				Task.Run(async () =>
				{
					var tcs = new TaskCompletionSource<bool>();
					tcs.SetResult(ResponseAwaits.MatchResponse(completedPacket));

					var isRequestPacket = await tcs.Task;

					if (isRequestPacket == false)
					{
						return;
					}

					// server
					BufferPool.Instance.ReturnBuffer(completedPacket);

					var mediatorContext = mediator.GetMediatorContext(interfaceName, methodName);
					if (mediatorContext == null)
					{
						throw new NullReferenceException("Method is not registered. Method name : " + methodName);
					}

					using (var argStream = new MemoryStream(argsBuffer))
					{
						var arg = ProtoBuf.Serializer.NonGeneric.Deserialize(mediatorContext.ArgumentType, argStream);

						// execute service interface method.
						var result = mediatorContext.Execute.DynamicInvoke(arg);
						
						var responsePacket = GeneratePacket(string.Empty, string.Empty, result, preamble);
						await responseSocket.SendPacketAsync(responsePacket);

						BufferPool.Instance.ReturnBuffer(responsePacket);
					}

					BufferPool.Instance.ReturnBuffer(argsBuffer);
				});

				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}

		public static T ParseArgument<T>(byte[] packet)
			where T : class
		{
			var argBuffer = GetArgBuffer(packet);

			using (var argStream = new MemoryStream(argBuffer))
			{
				var arg = ProtoBuf.Serializer.Deserialize<T>(argStream);
				return arg;
			}
		}

		public static object ParseArgument(byte[] packet, Type targetType)
		{
			var argBuffer = GetArgBuffer(packet);

			using (var argStream = new MemoryStream(argBuffer))
			{
				var arg = ProtoBuf.Serializer.NonGeneric.Deserialize(targetType, argStream);
				return arg;
			}
		}

		private static void ReadBuffer(byte[] sourceBuffer, byte[] copyBuffer, int startPosition)
		{
			var size = copyBuffer.Length;
			for (var i = 0; i < size; i++)
			{
				copyBuffer[i] = sourceBuffer[i + startPosition];
			}
		}

		// TODO : need packet parser.

		private static byte[] GetArgBuffer(byte[] packet)
		{
			var headerBuffer = BufferPool.Instance.GetBuffer(HeaderUnitSize);
			var start = 8; // skip size header.

			ReadBuffer(packet, headerBuffer, start);
			var interfaceNameSize = BitConverter.ToInt32(headerBuffer, 0);
			start += HeaderUnitSize;

			ReadBuffer(packet, headerBuffer, start);
			var methodNameSize = BitConverter.ToInt32(headerBuffer, 0);
			start += HeaderUnitSize;

			ReadBuffer(packet, headerBuffer, start);
			var argSize = BitConverter.ToInt32(headerBuffer, 0);
			start += HeaderUnitSize;

			// jump interface name
			start += interfaceNameSize;

			// jump method name
			start += methodNameSize;

			var argBuffer = BufferPool.Instance.GetBuffer(argSize);
			ReadBuffer(packet, argBuffer, start);
			return argBuffer;
		}
	}
}
