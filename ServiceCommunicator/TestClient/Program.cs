using System;
using System.Threading.Tasks;
using Communication;
using Define.Classes.Args;
using Define.Interfaces;

namespace TestClient
{
    class Program
    {
        const int ServicePort = 8009;
        const string ServiceIp = "172.16.10.70";
        //const string ServiceIp = "localhost";

        static void Main(string[] args)
        {
            Console.WriteLine("Ready to Start!");

            Console.Write("Server IP : ");
            var ipInput = Console.ReadLine();

            var communicator = new Communicator();
            communicator.ConnectToService(ipInput, ServicePort);

            var clientId = communicator.ClientId;

            Console.WriteLine("Connect to Service Start! IP : {0}, Port : {1}", ServiceIp, ServicePort);

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
                        for (int i = 0; i < 50; i++)
                        {
                            SendTest(communicator, clientId);

                            _sendCount++;
                        }
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

        private static async Task SendTest(Communicator com, Guid clientId)
        {
            var hash = _sendCount;
            var request = new SetServiceStatusRequest
            {
                ClientHash = hash,
                Status = true
            };

            Console.WriteLine("[Start Send] Hash : {0}", hash);

            var response = await com.Send((IServiceStatus service) => service.SetServiceStatus(request), clientId);
            if (response == null)
            {
                Console.WriteLine("[Response is null] Hash : {0}", hash);
                return;
            }

            Console.WriteLine("Received Response. IsSuccess : {0}, ClientHash : {1}", response.IsSuccess, response.ClientHash);
        }
    }
}
