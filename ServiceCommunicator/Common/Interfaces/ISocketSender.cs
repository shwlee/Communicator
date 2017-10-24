using System;
using System.Threading.Tasks;

namespace Communication.Common.Interfaces
{
	public interface ISocketSender
	{
		Task<int> SendAsync(byte[] packet, Guid clientId = default(Guid));

		int Send(byte[] packet, Guid clientId = default(Guid));

		void Disconnect(IStateObject state);
	}
}
