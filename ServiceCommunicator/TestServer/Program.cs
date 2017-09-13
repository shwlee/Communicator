using System;
using Communication;
using Define.Classes;
using Define.Classes.Args;
using Define.Interfaces;

namespace TestServer
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

            var isContinue = true;
            while (isContinue)
            {
                var input = Console.ReadLine();
                switch (input)
                {
                    case "c": // test memory
		                SendTest(communicator);
						GC.Collect();
                        break;
                    case "":
                    case "q":
                        isContinue = false;
                        break;
                }
            }

            communicator.Dispose();
            Console.ReadLine();
        }

	    static async void SendTest(Communicator com)
	    {
			var result = await com.SendAsync((IServiceStatus s) => s.KeepAlive(new Ping { SendTimeStamp = DateTime.UtcNow }));
		    Console.WriteLine(result);
		}
    }
}
