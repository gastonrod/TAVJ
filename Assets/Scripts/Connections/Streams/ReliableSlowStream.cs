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
            return messagesToSend;
        }

        public void SaveReceivedData(IPDataPacket data)
        {
            byte[] message = data.message;
            byte packetId = message[0];
            switch (message[1])
            {
                case (byte) PacketTypes.SPAWNED_PLAYER:
                    EnqueueGottenMessage( new byte[]{message[2], message[3]}, packetId, data.ip);
                    break;
                case (byte)PacketTypes.INIT_CONNECTION:
                    EnqueueGottenMessage( new byte[]{message[2]}, packetId, data.ip);
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