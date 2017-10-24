
namespace Communication.Common.Interfaces
{
	public interface IBufferPool
	{
		byte[] GetBuffer(int size);

		void ReturnBuffer(byte[] buffer);
	}
}
