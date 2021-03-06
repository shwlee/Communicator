﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communication.Hydrations;
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
            var serviceStatus = new ServiceStatus();
            var instanceMediator = new InstanceMediator();
            instanceMediator.SetInstance(serviceStatus);
            //var clientHash = ProtocolHash.GetProtocolHash();
            //var packets = PacketGenerator.GeneratePacket(
            //    "IServiceStatus",
            //    "SetServiceStatus",
            //    new SetServiceStatusRequest { ClientHash = clientHash, Status = true },
            //    clientHash);
            //var resultPacket = instanceMediator.Execute(packets);

            Console.ReadKey();

            var context = instanceMediator.GetMediatorContext("IServiceStatus", "GetServiceStatus");
            var result = context.Execute.DynamicInvoke(new GetServiceStatusRequest());		
			
            Console.ReadKey();
        }
    }
}
