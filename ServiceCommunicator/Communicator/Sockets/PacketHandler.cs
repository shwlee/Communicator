using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common.Threading;
using Communication.AsyncResponse;
using Mediator;

namespace Communication.Sockets
{
    /// <summary>
    /// Packet tokenizing and response from received Packet.
    /// </summary>
    public class PacketHandler
    {
        private static StaSynchronizationContext _socketSycnContext = new StaSynchronizationContext("SocketSynchronizationContext");

        private Socket _responseSocket;

        private InstanceMediator _mediator;

        private byte[] _buffer = new byte[StateObject.BUFFER_SIZE * 10];

        private int _readPosition;

        private int _lastPosition;

        public PacketHandler(Socket resposeSocket, InstanceMediator mediator)
        {
            this._responseSocket = resposeSocket;
            this._mediator = mediator;
        }

        public async Task HandlePackets(byte[] packet, int readBytes)
        {
            this._readPosition = 0;

            this.AddBuffer(packet, readBytes);

            // larger than packet header size. 
            // preamble : request hash (4),
            // header : interface name size (4) + method name size (4) + arg size (4),
            if (this._lastPosition < 16)
            {
                // continue read packets from socket.
                return;
            }

            var needCompaction = false;

            using (var bufferStream = new MemoryStream(this._buffer))
            {
                using (var binaryReader = new BinaryReader(bufferStream))
                {
                    while (this._readPosition != this._lastPosition)
                    {
                        try
                        {
                            var readStart = this._readPosition;
                            var preambleBytes = binaryReader.ReadBytes(4);
                            var preamble = BitConverter.ToInt32(preambleBytes, 0);
                            this._readPosition += 4;

                            var interfaceNameSizeBytes = binaryReader.ReadBytes(4);
                            var interfaceNameSize = BitConverter.ToInt32(interfaceNameSizeBytes, 0);
                            this._readPosition += 4;

                            var methodNameSizeBytes = binaryReader.ReadBytes(4);
                            var methodNameSize = BitConverter.ToInt32(methodNameSizeBytes, 0);
                            this._readPosition += 4;

                            var argSizeBytes = binaryReader.ReadBytes(4);
                            var argSize = BitConverter.ToInt32(argSizeBytes, 0);
                            this._readPosition += 4;

                            if (this._lastPosition < (this._readPosition + interfaceNameSize))
                            {
                                break;
                            }
                            var interfaceNameBytes = binaryReader.ReadBytes(interfaceNameSize);
                            var interfaceName = Encoding.UTF8.GetString(interfaceNameBytes);
                            this._readPosition += interfaceNameBytes.Length;

                            if (this._lastPosition < this._readPosition + methodNameSize)
                            {
                                break;
                            }
                            var methodNameBytes = binaryReader.ReadBytes(methodNameSize);
                            var methodName = Encoding.UTF8.GetString(methodNameBytes);
                            this._readPosition += methodNameBytes.Length;

                            var mediatorContext = this._mediator.GetMediatorContext(interfaceName, methodName);
                            if (this._lastPosition < (this._readPosition + argSize))
                            {
                                break;
                            }
                            var argBytes = binaryReader.ReadBytes(argSize);
                            this._readPosition += argSize;

                            var packetLength = this._readPosition - readStart;
                            var completedPacket = new byte[packetLength];
                            Array.Copy(this._buffer, readStart, completedPacket, 0,  packetLength);
                            
                            needCompaction = true;

                            var capPacket = completedPacket;
                            var capPreamble = preamble;
                            Task.Run(async () =>
                            {
                                var hash = capPacket;
                                var tcs = new TaskCompletionSource<bool>();
                                _socketSycnContext.Post(d =>
                                {
                                    tcs.SetResult(ResponseAwaits.MatchResponse(hash));
                                });

                                var isServiceCall = await tcs.Task;
                                if (isServiceCall == false)
                                {
                                    return;
                                }

                                if (mediatorContext == null)
                                {
                                    throw new NullReferenceException("Method is not registered. Method name : " + methodName);
                                }

                                using (var argStream = new MemoryStream(argBytes))
                                {
                                    var arg = ProtoBuf.Serializer.NonGeneric.Deserialize(mediatorContext.ArgumentType, argStream);
                                    var result = mediatorContext.Execute.DynamicInvoke(arg);

                                    var responsePacket = PacketGenerator.GeneratePacket(string.Empty, string.Empty, result, capPreamble);
                                    await this._responseSocket.SendTaskAsync(responsePacket);
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        } 
                    }
                }
            }

            if (needCompaction)
            {
                this.CompactBuffer();
            }
        }

        private void AddBuffer(byte[] packet, int readBytes)
        {
            Array.Copy(packet, 0, this._buffer, this._lastPosition, readBytes);
            this._lastPosition += readBytes;
        }

        private void CompactBuffer()
        {
            var remainSize = this._lastPosition - this._readPosition;
            var readPosition = this._readPosition;
            var lastPosition = this._lastPosition;

            this._readPosition = 0;
            this._lastPosition = remainSize;

            // read completed all packets.
            if (remainSize == 0)
            {
                Array.Clear(this._buffer, 0, lastPosition);
                return;
            }

            // buffer compaction.
            Array.Copy(this._buffer, readPosition, this._buffer, 0, remainSize);
        }

        public void Dispose()
        {
            Array.Clear(this._buffer, 0, this._lastPosition);
            this._readPosition = 0;
            this._lastPosition = 0;

            // don't dispose, just null;
            this._responseSocket = null;
            this._mediator = null;
        }
    }
}
