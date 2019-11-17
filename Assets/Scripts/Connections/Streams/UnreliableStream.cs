using System.Collections.Generic;
using System.Net;
using Connections.Loggers;

namespace Connections.Streams
{
    public class UnreliableStream :IStream
    {
        private ILogger _logger;
        /*
         *  Packet architecture:
         *  |     1B   |     1B     |       12B      |
         *  | ObjectID | ObjectType | ObjectPosition | 
         */

        public static readonly int PACKET_SIZE = 14;
        public UnreliableStream(ILogger logger)
        {
            _logger = logger;
        }
        
        private Queue<IPDataPacket> messagesToReceive = new Queue<IPDataPacket>();
        private Queue<IPDataPacket> messagesToSend = new Queue<IPDataPacket>();
        
        public void SaveMessageToSend(byte[] message, IPEndPoint ip)
        {
            messagesToSend.Enqueue(new IPDataPacket(ip, message));
        }

        public Queue<IPDataPacket> GetMessageToSend()
        {
            return messagesToSend;
        }

        public void SaveReceivedData(IPDataPacket data)
        {
            messagesToReceive.Enqueue(data);
        }

        public Queue<IPDataPacket> GetReceivedData()
        {
            return messagesToReceive;
        }
    }
}