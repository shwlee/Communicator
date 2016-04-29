using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mediator
{
	public class InstanceMediator
	{
		private Dictionary<string, List<MediatorContext>> _interfaceContexts = new Dictionary<string, List<MediatorContext>>();

		public void SetInstance(params object[] instances)
		{
			foreach (var instance in instances)
			{
				var type = instance.GetType();
				var interfaces = type.GetInterfaces();
				foreach (var @interface in interfaces)
				{
					if (this._interfaceContexts.ContainsKey(@interface.Name))
					{
						continue;
					}

					var mediatorContexts = new List<MediatorContext>();
					this._interfaceContexts.Add(@interface.Name, mediatorContexts);

					var methodInfos = @interface.GetMethods();
					foreach (var methodInfo in methodInfos)
					{
						var parameters = methodInfo.GetParameters();
						if (parameters.Length != 1) // allow only 1 parameter.
						{
							continue;
						}

						var context = new MediatorContext();
						context.TargetInstance = instance;
						context.InterfaceType = @interface;
						context.Method = methodInfo.Name;
						context.ArgumentType = parameters[0].ParameterType;
						//context.ReturnType = methodInfo.ReturnType;

						var parameterExpresssion = Expression.Parameter(parameters[0].ParameterType);
						var call = Expression.Call(Expression.Constant(instance), methodInfo, parameterExpresssion);
						context.Execute = Expression.Lambda(call, parameterExpresssion).Compile();

						mediatorContexts.Add(context);
					}
				}
			}			
		}

		// test TODO : delete this method.
		public MediatorContext GetMediatorContext(string interfaceName, string method)
		{
			if (this._interfaceContexts.ContainsKey(interfaceName) == false)
			{
				return null;
			}

			return this._interfaceContexts[interfaceName].FirstOrDefault(m => m.Method.Equals(method));
		}

		// TODO : need packet parser.

		private object Execute<T> (string interfaceName, string method, T parameter)
			where T : class			
		{
			if (this._interfaceContexts.ContainsKey(interfaceName) == false)
			{
				// or throw exception?
				return null;
			}

			var context = this._interfaceContexts[interfaceName].FirstOrDefault(m => m.Method.Equals(method));
			if (context == null)
			{
				// or throw exception?
				return null;
			}
			
			try
			{
				return context.Execute.DynamicInvoke(parameter);
			}
			catch (Exception)
			{
				// TODO : need logging.
				throw;
			}			
		}
	}
}
