using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communication.Hydrations
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
