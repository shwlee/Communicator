using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Communication;
using Define.Classes.Args;
using Define.Interfaces;

namespace Client
{
    class Program
    {
        const int ServicePort = 8009;
        const string ServiceIp = "172.16.20.70";

        static void Main(string[] args)
        {
            Console.WriteLine("Ready to Start!");

            var communicator = new Communicator();
            communicator.ConnectToService("172.16.20.70", ServicePort);

            Console.WriteLine("Connect to Service Start! IP : {0}, Port : {1}", ServiceIp, ServicePort);

            Console.ReadLine();

            SendTest(communicator);

            Console.ReadLine();
        }

        private static async Task SendTest(Communicator com)
        {
            var response = await com.Send((IServiceStatus service) => service.SetServiceStatus(new SetServiceStatusRequest
            {
                ClientHash = 20,
                Status = true
            }));
            
            Console.WriteLine("Received Response. IsSuccess : {0}", response.IsSuccess);
        }

        private static async Task<TResult> SendAsync<TInterface, TResult>(Expression<Func<TInterface, TResult>> method, Communicator com)
            where TInterface : class
            where TResult : class
        {
            var response = await com.Send(method);

            return response;
        }
    }
}
