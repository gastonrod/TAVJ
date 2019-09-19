using System.Collections.Generic;
using System.Net;
using ILogger = Connections.Loggers.ILogger;

namespace Connections.Streams
{
    // TODO: Re-send not acked packages.
    public class ReliableSlowStream : IStream
    {
        private ILogger _logger;
        private byte _lastPacketID = 0;
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
            byte packetID = message[0];
            switch (message[1])
            {
                case (byte)PacketTypes.SPAWNED_PLAYER:
                case (byte)PacketTypes.INIT_CONNECTION:
                    SendAck(packetID, data.ip);
                    byte[] decapsulatedMessage = {message[2]};
                    messagesToReceive.Enqueue(new IPDataPacket(data.ip, decapsulatedMessage));
                    break;
                case (byte)PacketTypes.ACK:
                    messagesNotAcked.Remove(packetID);
                    break;
            }
        }

        public Queue<IPDataPacket> GetReceivedData()
        {
            return messagesToReceive;
        }

        public void InitConnection(byte clientID, IPEndPoint ip)
        {
            byte[] message = {_lastPacketID++, (byte) PacketTypes.INIT_CONNECTION, clientID};
            SaveMessageToSend(message, ip);
        }

        public void SpawnPlayer(byte objectID, IPEndPoint ip)
        {
            byte[] message = {_lastPacketID++, (byte) PacketTypes.SPAWNED_PLAYER, objectID};
            SaveMessageToSend(message, ip);
        }
        
        public void SendAck(byte packetID, IPEndPoint ip)
        {
            byte[] ack =  {packetID, (byte)PacketTypes.ACK};
            SaveMessageToSend(ack, ip);
        }
    }
}