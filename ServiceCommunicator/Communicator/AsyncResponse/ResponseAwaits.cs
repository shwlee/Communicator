using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Communication.AsyncResponse
{
    public class ResponseAwaits
    {
        private static Dictionary<int, TaskCompletionSource<byte[]>> _reponseCollection = new Dictionary<int, TaskCompletionSource<byte[]>>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Insert(int hash, TaskCompletionSource<byte[]> tcs)
        {
            _reponseCollection.Add(hash, tcs);
        }

        /// <summary>
        /// Check the packet is call service or response from preamble packet.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>true if call service packet; or if it's response packet, return false.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool MatchResponse(byte[] packet)
        {
            var checkPacket = new byte[4];
            Buffer.BlockCopy(packet, 8, checkPacket, 0, 4); // jump size header and fill preamble to checkPacket.
            var preamble = BitConverter.ToInt32(checkPacket, 0);

            if (_reponseCollection.ContainsKey(preamble) == false)
            {
                // it's receive service call.				
                return true;
            }

            var tcs = _reponseCollection[preamble];

            // remove to handled response.
            _reponseCollection.Remove(preamble);

            tcs.SetResult(packet);

            return false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static TaskCompletionSource<byte[]> GetResponseSource(int hash)
        {
            if (_reponseCollection.ContainsKey(hash) == false)
            {
                // it's receive service call.				
                return null;
            }

            var tcs = _reponseCollection[hash];

            // remove to handled response.
            _reponseCollection.Remove(hash);

            return tcs;
        }
    }
}
