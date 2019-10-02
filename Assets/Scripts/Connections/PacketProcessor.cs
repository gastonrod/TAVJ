using System;
using System.Collections.Generic;
using System.Net;
using Connections.Streams;
using ILogger = Connections.Loggers.ILogger;

namespace Connections
{
    public class PacketProcessor
    {
        private int _destinationPort;
        private Connection _connection;
        private IStream[] _streams;
        private ILogger _logger;

        public PacketProcessor(Connection connection, ReliableSlowStream rss, ReliableFastStream rfs, UnreliableStream us, ILogger logger)
        {
            _connection = connection;
            _destinationPort = -1;
            _streams = new IStream[]{rss, rfs, us};
            _logger = logger;
        }
        public PacketProcessor(Connection connection, int destinationPort, ReliableSlowStream rss, ReliableFastStream rfs, UnreliableStream us, ILogger logger)
        {
            _connection = connection;
            _destinationPort = destinationPort;
            _streams = new IStream[]{rss, rfs, us};
            _logger = logger;
        }
        public void Update()
        {
            ReceiveMessages();
            SendMessages();
        }

        /*
         * Packet processor protocol:
         * For every Stream, message =
         *  |       1B         |       2B         |   xB   |
         *  |Stream identifier | Message length=x | message |
         */
        private readonly int PACKET_OVERHEAD = 3;
        private void SendMessages()
        {
            for (byte i = 0; i < _streams.Length; i++)
            {
                // Get streams messages
                Queue<IPDataPacket> messages = _streams[i].GetMessageToSend();
                while (messages.Count != 0)
                {
                    IPDataPacket packet = messages.Dequeue();
                    byte[] message = new byte[packet.message.Length+PACKET_OVERHEAD];
                    // Stream ID
                    message[0] = i;
                    // Message length
                    message[1] = (byte) packet.message.Length;
                    message[2] = (byte) (packet.message.Length >> 8);
                    
                    Array.Copy(packet.message, 0, message, PACKET_OVERHEAD, packet.message.Length);
                    _connection.SendData(message, packet.ip);
                }
            }
        }

        private void ReceiveMessages()
        {
            for(byte[] receivedData = _connection.ReceiveData();receivedData != null && receivedData.Length > 0; receivedData = _connection.ReceiveData())
            {
                {
                    for (int i = 0; i < receivedData.Length;)
                    {
                        // The IP Address is attached at Connection level. I unpack it here. Maybe do it in connection?
                        byte[] ipAddress =
                            {receivedData[i], receivedData[i + 1], receivedData[i + 2], receivedData[i + 3]};
                        i += 4;
                        int destPort = _destinationPort;
                        if (_destinationPort == -1)
                        {
                            destPort = BitConverter.ToInt16(receivedData, i);
                        }

                        i += 2;
                        byte streamID = receivedData[i++];

                        // Transform 2 bytes into message size
                        int messageSize = BitConverter.ToInt16(receivedData, i);
                        i += 2;

                        // Build the IPDataPacket
                        byte[] message = new byte[messageSize];
                        IPEndPoint ipEndPoint = new IPEndPoint(new IPAddress(ipAddress), destPort);
                        Array.Copy(receivedData, i, message, 0, messageSize);
                        i += messageSize;
                        IPDataPacket ipDataPacket = new IPDataPacket(ipEndPoint, message);
                        _streams[streamID].SaveReceivedData(ipDataPacket);
                    }
                }
            }
            
        }
    }
}