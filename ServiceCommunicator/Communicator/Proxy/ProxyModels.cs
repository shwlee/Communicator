
namespace Communication.Proxy
{
	public interface IProxyContext<T> : IProxyContainer
	{
		T Proxy { get; }
	}

	public interface IProxyContainer
	{
		bool IsMatchType<T>();
	}
}
