using System;

namespace Communication.Common.Interfaces
{
	public interface IStateObject : IDisposable
	{
		Guid ClientId { get; }
	}
}
