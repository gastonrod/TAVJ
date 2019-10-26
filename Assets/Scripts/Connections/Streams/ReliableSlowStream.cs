using System;
using System.Collections.Generic;
using System.Net;
using ILogger = Connections.Loggers.ILogger;

namespace Connections.Streams
{
    // TODO: Re-send not acked packages.
    public class ReliableSlowStream : IStream
    {
        private ILogger _logger;
        private byte _lastPacketId = 0;
        private Dictionary<byte, IPDataPacket> messagesNotAcked = new Dictionary<byte, IPDataPacket>();
        private Dictionary<IPEndPoint, byte[]> messagesAcked = new Dictionary<IPEndPoint, byte[]>();
        private byte MESSAGE_IS_ACKED = byte.MaxValue;
        private int currentSecond;

        public ReliableSlowStream(ILogger logger)
        {
            _logger = logger;
        }
        
        private enum PacketTypes
        {
            SPAWNED_PLAYER = 0,
            ACK,
            INIT_CONNECTION
        };
        
        private Queue<IPDataPacket> messagesToSend = new Queue<IPDataPacket>();
        private Queue<IPDataPacket> messagesToReceive = new Queue<IPDataPacket>();

        public void SaveMessageToSend(byte[] message, IPEndPoint ip)
        {
            IPDataPacket ipDataPacket = new IPDataPacket(ip, message);
            messagesToSend.Enqueue(ipDataPacket);
            messagesNotAcked[message[0]] = ipDataPacket;
        }

        public Queue<IPDataPacket> GetMessageToSend()
        {
            if (DateTime.Now.Second > currentSecond)
            {
                foreach (KeyValuePair<byte, IPDataPacket> keyValuePair in messagesNotAcked)
                {
                    messagesToSend.Enqueue(keyValuePair.Value);
                }
                currentSecond = DateTime.Now.Second % 60;
            }

            return messagesToSend;
        }

        public void SaveReceivedData(IPDataPacket data)
        {
            byte[] message = data.message;
            byte packetId = message[0];
            if (!messagesAcked.ContainsKey(data.ip))
            {
                messagesAcked[data.ip] = new byte[byte.MaxValue];
            }
            switch (message[1])
            {
                case (byte) PacketTypes.SPAWNED_PLAYER:
                    if (messagesAcked[data.ip][packetId] != MESSAGE_IS_ACKED)
                    {
                        messagesAcked[data.ip][packetId] = MESSAGE_IS_ACKED;
                        EnqueueGottenMessage( new []{message[2], message[3]}, packetId, data.ip);
                    }
                    break;
                case (byte)PacketTypes.INIT_CONNECTION:
                    if (messagesAcked[data.ip][packetId] != MESSAGE_IS_ACKED)
                    {
                        messagesAcked[data.ip][packetId] = MESSAGE_IS_ACKED;
                        EnqueueGottenMessage(new [] {message[2]}, packetId, data.ip);
                    }
                    break;
                case (byte)PacketTypes.ACK:
                    messagesNotAcked.Remove(packetId);
                    break;
            }
        }

        private void EnqueueGottenMessage(byte[] decapsulatedMessage, byte packetId, IPEndPoint ip)
        {
            SendAck(packetId, ip);
            messagesToReceive.Enqueue(new IPDataPacket(ip, decapsulatedMessage));
        }

        public Queue<IPDataPacket> GetReceivedData()
        {
            return messagesToReceive;
        }

        public void InitConnection(byte clientId, IPEndPoint ip)
        {
            byte[] message = {_lastPacketId++, (byte) PacketTypes.INIT_CONNECTION, clientId};
            SaveMessageToSend(message, ip);
        }

        public void SpawnPlayer(byte objectId, byte movementSpeed, IPEndPoint ip)
        {
            byte[] message = {_lastPacketId++, (byte) PacketTypes.SPAWNED_PLAYER, objectId, movementSpeed};
            SaveMessageToSend(message, ip);
        }
        
        private void SendAck(byte packetId, IPEndPoint ip)
        {
            byte[] ack =  {packetId, (byte)PacketTypes.ACK};
            SaveMessageToSend(ack, ip);
        }
    }
}