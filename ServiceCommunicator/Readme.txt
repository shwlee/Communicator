usage:

- using client.

1. Gets the Communicator instance.
var communicator = new Communicator();

2. Sets the service interface to Communicator instance if you need.
public class ServiceStatus : IServiceStatus
{
	public GetServiceStatusResponse GetServiceStatus(GetServiceStatusRequest request)
	{		
		return new GetServiceStatusResponse { IsOnLine = true, IsSuccess = true };
	}

	public SetServiceStatusResponse SetServiceStatus(SetServiceStatusRequest request)
	{	
		var clientHash = request.ClientHash;
		return new SetServiceStatusResponse { IsSuccess = true, ClientHash = ++clientHash };
	}

	public Pong KeepAlive(Ping request)
	{
		var sendTime = request.SendTimeStamp;
		return new Pong { IsSuccess = true, ReceivedTimeStamp = DateTime.UtcNow };
	}
}
var serviceStatus = new ServiceStatus();
communicator.Initialize(serviceStatus);

3. Connect to server.
communicator.ConnectToService(ipInput, ServicePort);

- This is sync call in Connect(). If you want async connect to server, call the ConnectToServiceAsync().
communicator.ConnectToServiceAsync(ipInput, ServicePort); // it's an awaitable method.


3. Call the service interface method.
3-1 by sync.
3-1-1. Gets the ServerProxy.
var serverProxy = communicator.ServerProxy<IServiceStatus>();


3-1-2. Call the interface method.
var ping = new Ping
{
	ClientHash = hash,
	ClientId = com.ClientId,
	SendTimeStamp = DateTime.UtcNow
}
var response = serverProxy.KeepAlive(ping);
- it will return the 'Pong' class type instance.

3-2 by async
Send to interface method with Expression to CallAsync() method.
var response = await communicator.CallAsync((IServiceStatus s) => s.KeepAlive(ping));
- there is wrapped that gets the ServerProxy procedure in CallAsync().
- it will return the 'Pong' class type instance by async.
- if you don't use await keyword, it will return Task<T> instance.







