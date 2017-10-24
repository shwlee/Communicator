using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading.Tasks;
using Communication.Common.Buffers;
using Communication.Common.Interfaces;
using Communication.Core.AsyncResponse;
using Communication.Core.Hydrations;
using Communication.Core.Packets;
using Communication.Common.Packets;

namespace Communication.Core.Proxy
{
	public class ServiceProxy<T> : RealProxy, IDisposable
	{
		private ISocketSender _socket;

		public ServiceProxy(ISocketSender socket) : base(typeof (T))
		{
			this._socket = socket;
		}

		public override IMessage Invoke(IMessage msg)
		{
			var message = msg as IMethodCallMessage;
			if (message?.MethodBase.DeclaringType == null)
			{
				return null;
			}

			try
			{
				var args = this.SendMessage(message);

				return new ReturnMessage(args, null, 0, message.LogicalCallContext, message);
			}
			catch (Exception ex)
			{
				return new ReturnMessage(ex, message);
			}
		}

		private object SendMessage(IMethodCallMessage message)
		{
			// get protocol hash and TaskCompletionSource save.
			var tcs = new TaskCompletionSource<byte[]>();
			var hash = ProtocolHash.GetProtocolHash();

			ResponseAwaits.Insert(hash, tcs); // TODO : consider synchronization problem.

			if (message.MethodBase.DeclaringType == null)
			{
				throw new NullReferenceException("Service interface type is null.");
			}

			var packet = new Message
			{
				InterfaceName = message.MethodBase.DeclaringType.Name,
				MethodName = message.MethodName,
				Argument = message.InArgs[0],
				RequestHash = hash
			};

			// get Packet.
			var sendBytes = PacketGenerator.GeneratePacket(packet);

			// check sender
			if (this._socket == null)
			{
				// disconnected or not connect yet.
				throw new NullReferenceException("Socket sender is null");
			}

			var sendPacketLength = this._socket.Send(sendBytes);
			BufferPool.Instance.ReturnBuffer(sendBytes);

			if (sendPacketLength == 0)
			{
				// disconnected.
				throw new ObjectDisposedException("Connection is disposed or disconnected.");
			}

			// receive response.
			var response = tcs.Task.Result;

			var returnType = message.MethodBase as MethodInfo;
			var args = PacketGenerator.ParseArgument(response, returnType?.ReturnType);

			BufferPool.Instance.ReturnBuffer(response);

			return args;
		}

		public void Dispose()
		{
			this._socket = null;
		}
	}
}
