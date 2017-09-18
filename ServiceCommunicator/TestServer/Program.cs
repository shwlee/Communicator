using Communication;
using Define.Classes;
using Define.Classes.Args;
using Define.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                var input = Console.ReadKey();
                switch (input.Key)
                {
                    case ConsoleKey.C: // test memory
		                SendTest(communicator);
						GC.Collect();
                        break;
                    case ConsoleKey.Spacebar:
                    case ConsoleKey.Q:
                        isContinue = false;
                        break;
                }
            }

            communicator.Dispose();
            Console.ReadLine();
        }

	    static async void SendTest(Communicator com)
	    {
		    var taskList = com.ConnectedClients.Select(clientId => com.CallToClientAsync((IServiceStatus s) => s.KeepAlive(new Ping
		    {
			    SendTimeStamp = DateTime.UtcNow
		    }), clientId)).ToList();

		    await Task.WhenAll(taskList);

		    foreach (var task in taskList)
		    {
			    Console.WriteLine(task.Result);
		    }
		}
    }
}
