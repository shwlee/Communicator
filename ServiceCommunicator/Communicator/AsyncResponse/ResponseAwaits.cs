using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Communication.Core.Packets;

namespace Communication.Core.AsyncResponse
{
	public class ResponseAwaits
    {
        private static readonly Dictionary<int, TaskCompletionSource<byte[]>> _responseCollection = new Dictionary<int, TaskCompletionSource<byte[]>>();
		
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Insert(int hash, TaskCompletionSource<byte[]> tcs)
        {
            _responseCollection.Add(hash, tcs);
        }

        /// <summary>
        /// Check the packet is call service or response from preamble packet.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>true if call service packet; or if it's response packet, return false.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool MatchResponse(byte[] packet)
        {
	        var preamble = PacketGenerator.GetPreamble(packet);

            if (_responseCollection.ContainsKey(preamble) == false)
            {
                // it's receive service call.				
                return true;
            }

            var tcs = _responseCollection[preamble];

            // remove to handled response.
            _responseCollection.Remove(preamble);

            tcs.SetResult(packet);

            return false;
        }

		[MethodImpl(MethodImplOptions.Synchronized)]
        public static TaskCompletionSource<byte[]> GetResponseSource(int hash)
        {
            if (_responseCollection.ContainsKey(hash) == false)
            {
                // it's receive service call.				
                return null;
            }

            var tcs = _responseCollection[hash];

            // remove to handled response.
            _responseCollection.Remove(hash);

            return tcs;
        }
    }
}
