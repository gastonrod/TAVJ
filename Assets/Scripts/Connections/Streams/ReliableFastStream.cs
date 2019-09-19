using System.Collections.Generic;
using System.Net;
using ILogger = Connections.Loggers.ILogger;

namespace Connections.Streams
{
    public class ReliableFastStream : IStream
    {
        private ILogger _logger;
        private byte _lastPacketID = 0;
        private Dictionary<byte, IPDataPacket> messagesNotAcked = new Dictionary<byte, IPDataPacket>();
        private Dictionary<IPEndPoint, byte> messagesAcked = new Dictionary<IPEndPoint, byte>();

        public ReliableFastStream(ILogger logger)
        {
            _logger = logger;
        }

        private Queue<IPDataPacket> messagesToReceive = new Queue<IPDataPacket>();
        private Queue<IPDataPacket> acksToSend = new Queue<IPDataPacket>();

        public void SaveMessageToSend(byte[] message, IPEndPoint ip)
        {
            IPDataPacket ipDataPacket = new IPDataPacket(ip, message);
            if (message.Length > 1)
            {
                messagesNotAcked[message[0]] = ipDataPacket;
            }
            else
            {
                acksToSend.Enqueue(ipDataPacket);
            }
        }

        public Queue<IPDataPacket> GetMessageToSend()
        {
            Queue<IPDataPacket> messagesToSend = new Queue<IPDataPacket>();
            foreach (KeyValuePair<byte, IPDataPacket> keyValuePair in messagesNotAcked)
            {
                messagesToSend.Enqueue(keyValuePair.Value);
            }

            while (acksToSend.Count > 0)
            {
                messagesToSend.Enqueue(acksToSend.Dequeue());
            }
            return messagesToSend;
        }

        public void SaveReceivedData(IPDataPacket data)
        {
            byte[] message = data.message;
            byte packetID = message[0];
            if (message.Length == 1)
            {
                messagesNotAcked.Remove(packetID);
            } else {
                if (!messagesAcked.ContainsKey(data.ip))
                {
                    messagesAcked[data.ip] = 0;
                }
                SendAck(packetID, data.ip);
                if (messagesAcked[data.ip] < packetID)
                {
                    byte[] decapsulatedMessage = {message[1], message[2]};
                    messagesToReceive.Enqueue(new IPDataPacket(data.ip, decapsulatedMessage));
                    messagesAcked[data.ip] = packetID;
                }
            }
        }

        public Queue<IPDataPacket> GetReceivedData()
        {
            return messagesToReceive;
        }
        
        private void SendAck(byte packetID, IPEndPoint ip)
        {
            byte[] ack = {packetID};
            SaveMessageToSend(ack, ip);
        }

        public void SendInput(byte inputCode, byte playerID, IPEndPoint ip)
        {
            byte[] message = {_lastPacketID++, playerID, inputCode};
            SaveMessageToSend(message, ip);
        }
    }
}