using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Communication.Packets
{
    class BufferPool
    {
        private const int INITIAL_POOL_SIZE = 512;

        internal const int BUFFER_SIZE = 1024;

        private static readonly Lazy<BufferPool> _singletonInstance = new Lazy<BufferPool>(() => new BufferPool());

        // pool of buffers
        private Queue<byte[]> _freeBuffers;

        // Singleton attribute
        public static BufferPool Instance
        {
            get
            {
                return _singletonInstance.Value;
            }
        }

        private BufferPool()
        {
            this._freeBuffers = new Queue<byte[]>(INITIAL_POOL_SIZE);
            for (var i = 0; i < INITIAL_POOL_SIZE; i++)
            {
                this._freeBuffers.Enqueue(new byte[BUFFER_SIZE]);
            }
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] GetBuffer()
        {
            if (this._freeBuffers.Count > 0)
            {
                if (this._freeBuffers.Count > 0)
                {
                    return this._freeBuffers.Dequeue();
                }
            }

            // instead of creating new buffer,
            // blocking waiting or refusing request may be better
            var extensionBuffer = new byte[BUFFER_SIZE];
            this._freeBuffers.Enqueue(extensionBuffer);

            return extensionBuffer;
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ReturnBuffer(byte[] buffer)
        {
            this._freeBuffers.Enqueue(buffer);
        }
    }
}
