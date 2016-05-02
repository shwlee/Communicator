using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Communication
{
	public class ResponseAwaits
	{
		private static Dictionary<int, TaskCompletionSource<byte[]>> _reponseCollection = new Dictionary<int, TaskCompletionSource<byte[]>>();

		public static void Insert(int hash, TaskCompletionSource<byte[]> tcs)
		{
			_reponseCollection.Add(hash, tcs);
		}

		public static void MatchResponse(byte[] packet)
		{
			var checkPacket = new byte[4];
			Array.Copy(packet, checkPacket, 4);
			var preamble = BitConverter.ToInt32(checkPacket, 0);

			if (_reponseCollection.ContainsKey(preamble) == false)
			{
				// it's receive callback.
				// TODO : implement
				return;
			}

			var tcs = _reponseCollection[preamble];

			// remove to handled response.
			_reponseCollection.Remove(preamble);

			tcs.SetResult(packet);
		}
	}
}
