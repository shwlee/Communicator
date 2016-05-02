using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Define.Classes;
using Define.Classes.Args;
using Define.Interfaces;
using Mediator;

namespace ServiceCommunicator
{
	class Program
	{
		static void Main(string[] args)
		{
			var mediator = new MediatorContext {InterfaceType = typeof(IServiceStatus), Method = "SetServiceStatus" };
			var clientHash = mediator.GetHashCode();
			var packets = PacketHelper.GeneratePacket(mediator.InterfaceName, mediator.Method, new SetServiceStatusRequest { ClientHash = clientHash, Status = true }, clientHash);
			PacketHelper.ParsePacket<SetServiceStatusRequest>(packets, typeof(SetServiceStatusRequest));

			Console.ReadKey();

			var serviceStatus = new ServiceStatus();
			var instanceMediator = new InstanceMediator();
			instanceMediator.SetInstance(serviceStatus);

			var context = instanceMediator.GetMediatorContext("IServiceStatus", "GetServiceStatus");
			var result = context.Execute.DynamicInvoke(new GetServiceStatusRequest());

			Console.ReadKey();
		}
	}
}
