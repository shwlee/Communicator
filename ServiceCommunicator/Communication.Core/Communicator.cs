using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Communication.Common.Buffers;
using Communication.Common.Interfaces;
using Communication.Core.Mediator;
using Communication.Core.Packets;
using Communication.Core.Proxy;
using Communication.Core.Sockets;

namespace Communication.Core
{
	public class Communicator
	{
		private static readonly InstanceMediator _mediator = new InstanceMediator();

		private ServiceSocket _service;
		
		// allow only 1 connect
		private OutgoingSocket _outgoing;

		private static readonly object _syncBlock = new object();

		private readonly Dictionary<CallFlow, List<IProxyContainer>> _syncProxies = new Dictionary<CallFlow, List<IProxyContainer>>();

		private readonly Dictionary<CallFlow, List<IProxyContainer>> _asyncProxies = new Dictionary<CallFlow, List<IProxyContainer>>();

		public Guid ClientId => this._outgoing?.ClientId ?? default(Guid);

		public List<Guid> ConnectedClients => this._service.ConnectedClients;

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

		/// <summary>
		/// Gets synchronous server proxy when request message client to server.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T ServerProxy<T>() where T : class
		{
			return this.Proxy<T>(CallFlow.Request);
		}

		/// <summary>
		/// Gets synchronous client proxy when request message server to client.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T ClientProxy<T>() where T : class
		{
			return this.Proxy<T>(CallFlow.Notify);
		}

		public async Task<TResult> CallToServerAsync<T, TResult>(Func<T, TResult> func)
			where T : class
			where TResult : class
		{
			return await this.AsyncProxy<T>(CallFlow.Request).CallAsync(func);
		}

		public async Task<TResult> CallToClientAsync<T, TResult>(Func<T, TResult> func, Guid clientId)
			where T : class
			where TResult : class
		{
			return await this.AsyncProxy<T>(CallFlow.Notify).CallAsync(func, clientId);
		}

		private T Proxy<T>(CallFlow flow) where T : class
		{
			// TODO : improvement pool structure

			lock (_syncBlock)
			{
				if (this._syncProxies.ContainsKey(flow) == false)
				{
					return this.AddProxy<T>(flow);
				}

				var proxyList = this._syncProxies[flow];
				var proxy = proxyList.FirstOrDefault(p => p.IsMatchType<T>());
				if (proxy == null)
				{
					return this.AddProxy<T>(flow);
				}

				var model = proxy as IProxyContext<T>;
				return model?.Proxy;
			}
		}
		
		private IAsyncProxy<T> AsyncProxy<T>(CallFlow flow) where T : class
		{
			// TODO : improvement pool structure

			lock (_syncBlock)
			{
				if (this._asyncProxies.ContainsKey(flow) == false)
				{
					return this.AddAsyncProxy<T>(flow);
				}

				var proxyList = this._asyncProxies[flow];
				var proxy = proxyList.FirstOrDefault(p => p.IsMatchType<T>());
				return proxy as IAsyncProxy<T> ?? this.AddAsyncProxy<T>(flow);
			}
		}

		private T AddProxy<T>(CallFlow flow) where T : class
		{
			var socketSender = flow == CallFlow.Notify ? (ISocketSender) this._service : this._outgoing;
			var newProxy = ServiceProxyFactory<T>.GetProxy(socketSender);
			var proxyContext = new ProxyContext<T>(newProxy);
			this._syncProxies.Add(flow, new List<IProxyContainer> {proxyContext});

			return newProxy;
		}

		private IAsyncProxy<T> AddAsyncProxy<T>(CallFlow flow) where T : class
		{
			var socketSender = flow == CallFlow.Notify ? (ISocketSender) this._service : this._outgoing;
			var newProxy = new AsyncServiceProxy<T>(socketSender);
			this._asyncProxies.Add(flow, new List<IProxyContainer> {newProxy});

			return newProxy;
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
