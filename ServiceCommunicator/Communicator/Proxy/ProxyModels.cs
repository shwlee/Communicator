using System;
using System.Threading.Tasks;

namespace Communication.Core.Proxy
{
	public interface IProxyContainer
	{
		bool IsMatchType<T>();
	}

	public interface IProxyContext<out T> : IProxyContainer
	{
		T Proxy { get; }
	}

	public interface IAsyncProxy<out T> : IProxyContainer
	{
		Task<TResult> CallAsync<TResult>(Func<T, TResult> func, Guid clientId = default(Guid)) where TResult : class;
	}
}
