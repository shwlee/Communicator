using System;
using System.Linq.Expressions;
using Common.Communication;

namespace Communication.Hydrations
{
	public class HydrateExpression
	{
		public static Message Get<TInterface, TResult>(Expression<Func<TInterface, TResult>> method)
			where TInterface : class
			where TResult : class
		{
			return Hydrate(method);
		}

		public static Message Get<TInterface>(Expression<Action<TInterface>> method)
			where TInterface : class
		{
			return Hydrate(method);
		}

		private static Message Hydrate<T>(Expression<T> method)
		{
			if (method.Body.NodeType != ExpressionType.Call)
			{
				throw new InvalidOperationException("Func expression is not ExpressionType.Call type");
			}

			var info = method.Body as MethodCallExpression;
			if (info == null)
			{
				throw new InvalidCastException("Func body is not MethodCallExpression type");
			}

			var methodName = info.Method.Name;
			var interfaceName = info.Method.DeclaringType.Name;
			object argObject = null;

			if (info.Arguments.Count > 0) // allow only 1 parameter.
			{
				// TODO : handle to exception.

				var hydrationArg = Expression.Lambda<Func<object>>(info.Arguments[0], null);

				var compiledArgObjectExpression = hydrationArg.Compile();
				argObject = compiledArgObjectExpression();				
			}

			return new Message { MethodName = methodName, InterfaceName = interfaceName, Argument = argObject };
		}
	}
}
