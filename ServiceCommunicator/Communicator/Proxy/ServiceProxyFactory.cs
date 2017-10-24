using System;
using Communication.Common.Interfaces;

namespace Communication.Core.Proxy
{
	public class ServiceProxyFactory<T> where T : class
	{
		private static readonly object _syncBlock = new object();

		private static T _proxy;

		public static T GetProxy(ISocketSender socket)
		{
			try
			{
				ServiceProxy<T> serviceProxy = null;
				lock (_syncBlock)
				{
					if (_proxy == null)
					{
						serviceProxy = new ServiceProxy<T>(socket);
					}
				}

				if (serviceProxy == null)
				{
					// TODO : throw
					return null;
				}

				_proxy = serviceProxy.GetTransparentProxy() as T;

				return _proxy;
			}
			catch (Exception ex)
			{
				// TODO : need logging.
				return null;
			}
		}
	}
}
