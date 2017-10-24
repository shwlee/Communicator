
namespace Communication.Core.Packets
{
    public enum PacketDirection
    {
        Incomming,
        Outgoing,
    }

	public enum CallFlow
	{
		// send to server.
		Request,

		// send to client.
		Notify
	}
}
