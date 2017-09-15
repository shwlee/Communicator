using Common.Communication;
using Common.Interfaces;
using Communication.Packets;
using Communication.Proxy;
using Communication.Sockets;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Communication
{
	public class Communicator
	{
		private static InstanceMediator _mediator = new InstanceMediator();

		private ServiceSocket _service;

		private static object _syncBlock = new object();

		// allow only 1 connect
		private OutgoingSocket _outgoing;

		private Dictionary<CallFlow, List<IProxyContainer>> _proxies = new Dictionary<CallFlow, List<IProxyContainer>>();

		public Guid ClientId
		{
			get
			{
				if (this._outgoing == null)
				{
					return default(Guid);
				}

				return this._outgoing.ClientId;
			}
		}

		public void Initialize(params object[] instances)
		{
			_mediator.SetInstance(instances);
		}

		public void StartService(int port, int backlog)
		{
			this._service = new ServiceSocket();
			this._service.StartService(port, backlog);
		}

		public void ConnectToService(string ip, int port)
		{
			this._outgoing = new OutgoingSocket();
			this._outgoing.Connect(ip, port);
		}

		public T Proxy<T>(CallFlow flow) where T : class
		{
			lock (_syncBlock)
			{
				if (this._proxies.ContainsKey(flow) == false)
				{
					var socketSender = flow == CallFlow.Notify ? (ISocketSender)this._service : this._outgoing;
					var newProxy = ServiceProxyFactory<T>.GetProxy(socketSender);

					IProxyContainer proxyContext = new ProxyContext<T>(newProxy);
					
					this._proxies.Add(flow, new List<IProxyContainer> { proxyContext });

					return newProxy;
				}

				var proxyList = this._proxies[flow];
				var proxy = proxyList.FirstOrDefault(p => p.IsMatchType<T>());
				var model = proxy as IProxyContext<T>;
				return model?.Proxy;
			}
		}

		internal static void ReceiveCallback(IAsyncResult ar)
		{
			Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~ Thread Id : " + Thread.CurrentThread.ManagedThreadId + " , IsPool : " + Thread.CurrentThread.IsThreadPoolThread);
			var stateObject = ar.AsyncState as StateObject;
			if (stateObject == null)
			{
				return;
			}

			var socket = stateObject.WorkSocket;
			try
			{
				var read = socket.EndReceive(ar);
				if (read > 0)
				{
					// TODO : need packet logging.

					Console.WriteLine("[Received] Read Packets : {0}", read);

					if (stateObject.PacketHandler == null)
					{
						stateObject.PacketHandler = new PacketHandler(socket, _mediator);
					}

					stateObject.PacketHandler.HandlePackets(stateObject.Buffer, read);
				}

				BufferPool.Instance.ReturnBuffer(stateObject.Buffer);

				stateObject.Buffer = null;
				
				stateObject.Buffer = BufferPool.Instance.GetBuffer(BufferPool.Buffer1024Size);

				socket.BeginReceive(stateObject.Buffer, 0, BufferPool.Buffer1024Size, SocketFlags.None, ReceiveCallback, stateObject);
			}
			catch (ObjectDisposedException ode)
			{
				Console.WriteLine("{0} Socket Disposed! {1}", stateObject.ClientId, ode);
				stateObject.Dispose();
				Console.WriteLine();
			}
			catch (SocketException se)
			{
				if (se.ErrorCode == 10054)
				{
					// TODO : add to connection close message and handle to socket management
					Console.WriteLine("{0} Socket Closed! {1}", stateObject.ClientId,se);
					stateObject.Dispose();
				}
				Console.WriteLine();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				socket.Close();
				Console.WriteLine();
			}
		}
		
		public void Dispose()
		{
			this._service?.Dispose();

			this._outgoing?.Dispose();
		}
	}
}
