
namespace Communication.Core.Proxy
{
	public class ProxyContext<T> : IProxyContext<T>
	{
		public T Proxy { get; set; }

		public ProxyContext(T proxy)
		{
			this.Proxy = proxy;
		}

		public bool IsMatchType<TProxy>()
		{
			return typeof(T) == typeof(TProxy);
		}
	}
}
