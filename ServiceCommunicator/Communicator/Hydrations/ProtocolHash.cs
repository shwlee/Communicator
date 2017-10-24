using System.Threading;

namespace Communication.Core.Hydrations
{
	public class ProtocolHash
	{
		private static int _hash;

		public static int GetProtocolHash()
		{
			Interlocked.Increment(ref _hash);

			return _hash;
		}
	}
}
