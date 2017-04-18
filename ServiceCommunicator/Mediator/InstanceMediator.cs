using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Mediator
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

		// TODO : need packet parser.
		public T ParseArgument<T>(byte[] packet)
			where T : class
		{
			using (var memoryStream = new MemoryStream(packet))
			{
				using (var binaryReader = new BinaryReader(memoryStream))
				{
                    // jump size header and preamble
                    binaryReader.ReadBytes(12);

					var interfaceNameSizeBytes = binaryReader.ReadBytes(4);
					var interfaceNameSize = BitConverter.ToInt32(interfaceNameSizeBytes, 0);

					var methodNameSizeBytes = binaryReader.ReadBytes(4);
					var methodNameSize = BitConverter.ToInt32(methodNameSizeBytes, 0);

					var argSizeBytes = binaryReader.ReadBytes(4);
					var argSize = BitConverter.ToInt32(argSizeBytes, 0);

                    // jump interface name
                    binaryReader.ReadBytes(interfaceNameSize);

                    // jump method name
				    binaryReader.ReadBytes(methodNameSize);

					var argBytes = binaryReader.ReadBytes(argSize);
					using (var argStream = new MemoryStream(argBytes))
					{
						var arg = ProtoBuf.Serializer.Deserialize<T>(argStream);
						return arg;
					}
				}
			}
		}

	    public object ParseArgument(byte[] packet, Type targetType)
	    {
            using (var memoryStream = new MemoryStream(packet))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    // jump size header and preamble
                    binaryReader.ReadBytes(12);

                    var interfaceNameSizeBytes = binaryReader.ReadBytes(4);
                    var interfaceNameSize = BitConverter.ToInt32(interfaceNameSizeBytes, 0);

                    var methodNameSizeBytes = binaryReader.ReadBytes(4);
                    var methodNameSize = BitConverter.ToInt32(methodNameSizeBytes, 0);

                    var argSizeBytes = binaryReader.ReadBytes(4);
                    var argSize = BitConverter.ToInt32(argSizeBytes, 0);

                    // jump interface name
                    binaryReader.ReadBytes(interfaceNameSize);

                    // jump method name
                    binaryReader.ReadBytes(methodNameSize);

                    var argBytes = binaryReader.ReadBytes(argSize);
                    using (var argStream = new MemoryStream(argBytes))
                    {
                        var arg = ProtoBuf.Serializer.NonGeneric.Deserialize(targetType, argStream);
                        return arg;
                    }
                }
            }
	    }

		public byte[] Execute(byte[] packet)
		{
			using (var memoryStream = new MemoryStream(packet))
			{
				using (var binaryReader = new BinaryReader(memoryStream))
				{
					var preambleBytes = binaryReader.ReadBytes(4);
					var preamble = BitConverter.ToInt32(preambleBytes, 0);

					var interfaceNameSizeBytes = binaryReader.ReadBytes(4);
					var interfaceNameSize = BitConverter.ToInt32(interfaceNameSizeBytes, 0);

					var methodNameSizeBytes = binaryReader.ReadBytes(4);
					var methodNameSize = BitConverter.ToInt32(methodNameSizeBytes, 0);

					var argSizeBytes = binaryReader.ReadBytes(4);
					var argSize = BitConverter.ToInt32(argSizeBytes, 0);

					var interfaceNameBytes = binaryReader.ReadBytes(interfaceNameSize);
					var interfaceName = Encoding.UTF8.GetString(interfaceNameBytes);

					if (this._interfaceContexts.ContainsKey(interfaceName) == false)
					{
						throw new NullReferenceException("Interface is not registered. Interface name : " + interfaceName);
					}
					
					var methodNameBytes = binaryReader.ReadBytes(methodNameSize);
					var methodName = Encoding.UTF8.GetString(methodNameBytes);

					var mediatorContext = this._interfaceContexts[interfaceName].FirstOrDefault(m => m.Method.Equals(methodName));
					if (mediatorContext == null)
					{
						throw new NullReferenceException("Method is not registered. Method name : " + methodName);
					}

					var argBytes = binaryReader.ReadBytes(argSize);

					using (var argStream = new MemoryStream(argBytes))
					{
						var arg = ProtoBuf.Serializer.NonGeneric.Deserialize(mediatorContext.ArgumentType, argStream);
						var result = mediatorContext.Execute.DynamicInvoke(arg);

						return PacketGenerator.GeneratePacket(interfaceName, methodName, result, preamble);
					}					
				}
			}
		}

		
	}
}
