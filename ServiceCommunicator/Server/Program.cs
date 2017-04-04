using Define.Classes;
using System;
using Communication;

namespace Server
{
    class Program
    {
        const int ServicePort = 8009;

        static void Main(string[] args)
        {
            var serviceStatus = new ServiceStatus();
            var communicator = new Communicator();
            communicator.Initialize(serviceStatus);
            communicator.StartService(ServicePort, int.MaxValue);

            Console.WriteLine("Service Start! Port : {0}", ServicePort);
            Console.ReadLine();

            Console.ReadKey();
        }
    }
}
