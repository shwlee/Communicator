using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Communication.Core.Mediator
{
	public class InstanceMediator
	{
		private Dictionary</*interface name*/string, List<MediatorContext>> _interfaceContexts = new Dictionary<string, List<MediatorContext>>();

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
						context.InterfaceType = @interface;
						context.Method = methodInfo.Name;
						context.ArgumentType = parameters[0].ParameterType;

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
	}
}
