using System;
using System.Threading.Tasks;

namespace Common.Threading
{
	public static class AsyncExtension
	{
		public static Task<TResult> AsAsync<TOwner, TResult>(this TOwner owner, Func<TOwner, TResult> func)
		{
			return Task.Run(() => func(owner));
		}

		public static Task<T> AsAsync<T>(this object obj, Func<T> func)
		{
			return Task.Run(func);
		}

		public static Task AsAsync(this object obj, Action action)
		{
			return Task.Run(action);
		}
	}
}
