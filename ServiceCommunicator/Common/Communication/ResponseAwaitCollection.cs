using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Communication
{
	public class ResponseAwaitCollection
	{
		private Dictionary<int, TaskCompletionSource<byte[]>> _reponseCollection;

		private static Lazy<ResponseAwaitCollection> _lazyInstance = new Lazy<ResponseAwaitCollection>(() => new ResponseAwaitCollection());
		public static ResponseAwaitCollection Instance
		{
			get { return _lazyInstance.Value; }
		}

		private ResponseAwaitCollection()
		{
			this._reponseCollection = new Dictionary<int, TaskCompletionSource<byte[]>>();
		}

		public void Insert(int hash, TaskCompletionSource<byte[]> tcs)
		{
			this._reponseCollection.Add(hash, tcs);
		}

		public void MatchResponse(byte[] packet)
		{
			var checkPacket = new byte[4];
			Array.Copy(packet, checkPacket, 4);
			var preamble = BitConverter.ToInt32(checkPacket, 0);

			if (this._reponseCollection.ContainsKey(preamble) == false)
			{
				// it's receive callback.
				// TODO : implement
				return;
			}

			var tcs = this._reponseCollection[preamble];

			// remove to handled response.
			this._reponseCollection.Remove(preamble);

			tcs.SetResult(packet);
		}
	}
}
