using System;
using System.Threading.Tasks;
using Communication.Core;
using Define.Classes;
using Define.Classes.Args;
using Define.Interfaces;

namespace TestClient
{
	class Program
	{
		const int ServicePort = 8009;
		//const string ServiceIp = "172.16.10.70";
		//const string ServiceIp = "localhost";

		static void Main(string[] args)
		{
			Console.WriteLine("Ready to Start!");

			Console.Write("Server IP : ");
			var ipInput = Console.ReadLine();

			var communicator = new Communicator();
			var serviceStatus = new ServiceStatus();
			communicator.Initialize(serviceStatus); // if need interface implement in client side.
			communicator.ConnectToService(ipInput, ServicePort);
			
			Console.WriteLine("Connect to Service Start! IP : {0}, Port : {1}", ipInput, ServicePort);

			var isContinue = true;
			while (isContinue)
			{
				var input = Console.ReadLine();
				switch (input)
				{
					case "c": // test memory
						GC.Collect();
						break;
					case "":
						//for (int i = 0; i < 50; i++)
						//{
						SendTest(communicator);

						_sendCount++;
						//}
						break;
					case "q":
						isContinue = false;
						break;
				}
			}

			Console.ReadLine();

			communicator.Dispose();

			Console.ReadLine();
		}

		private static int _sendCount;

		private static async Task SendTest(Communicator com)
		{
			var hash = _sendCount;

			// call asynchronous

			if (hash % 2 == 0)
			{
				var pingResponse = await com.CallToServerAsync((IServiceStatus s) => s.KeepAlive(new Ping
				{
					ClientHash = hash,
					ClientId = com.ClientId,
					SendTimeStamp = DateTime.UtcNow
				}));


				Console.WriteLine("[KeepAlive] Received Response. IsSuccess : {0}, Time : {1}", pingResponse.IsSuccess, pingResponse.ReceivedTimeStamp);
			}
			else
			{
				var setStatusResponse = await com.CallToServerAsync((IServiceStatus s) => s.SetServiceStatus(new SetServiceStatusRequest
				{
					ClientHash = hash,
					ClientId = com.ClientId,
					Status = true
				}));

				Console.WriteLine("[SetStatus] @@@@@@@@@@@@@@ Rceived Response. IsSuccess : {0}, Hash : {1}", setStatusResponse.IsSuccess, setStatusResponse.ClientHash);
			}

			// call synchronous

			//if (hash % 2 == 0)
			//{
			//	var pingResponse = com.ServerProxy<IServiceStatus>().KeepAlive(new Ping
			//	{
			//		ClientHash = hash,
			//		ClientId = com.ClientId,
			//		SendTimeStamp = DateTime.UtcNow
			//	});


			//	Console.WriteLine("[KeepAlive] Received Response. IsSuccess : {0}, Time : {1}", pingResponse.IsSuccess, pingResponse.ReceivedTimeStamp);
			//}
			//else
			//{
			//	var setStatusResponse = com.ServerProxy<IServiceStatus>().SetServiceStatus(new SetServiceStatusRequest
			//	{
			//		ClientHash = hash,
			//		ClientId = com.ClientId,
			//		Status = true
			//	});

			//	Console.WriteLine("[SetStatus] @@@@@@@@@@@@@@ Rceived Response. IsSuccess : {0}, Hash : {1}", setStatusResponse.IsSuccess, setStatusResponse.ClientHash);
			//}
		}
	}
}
