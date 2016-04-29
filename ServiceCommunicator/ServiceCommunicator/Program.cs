using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Define.Classes;
using Define.Classes.Args;
using Mediator;

namespace ServiceCommunicator
{
	class Program
	{
		static void Main(string[] args)
		{
			var serviceStatus = new ServiceStatus();
			var instanceMediator = new InstanceMediator();
			instanceMediator.SetInstance(serviceStatus);

			var context = instanceMediator.GetMediatorContext("IServiceStatus", "GetServiceStatus");
			var result = context.Execute.DynamicInvoke(new GetServiceStatusRequest());

			Console.ReadKey();
		}
	}
}
