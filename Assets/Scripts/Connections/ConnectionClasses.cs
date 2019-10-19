using Connections.Loggers;
using Connections.Streams;

namespace Connections
{
    public struct ConnectionClasses
    {
        public ReliableSlowStream rss;
        public ReliableFastStream rfs;
        public UnreliableStream us;
        public PacketProcessor pp;
        
        // Method used by server
        public ConnectionClasses(int sourcePort, int delayInMs, int packetLossPct, ILogger logger)
        {
            Connection connection = new Connection(sourcePort, delayInMs, packetLossPct);
            rss = new ReliableSlowStream(logger);
            rfs = new ReliableFastStream(logger);
            us = new UnreliableStream(logger);
            pp  = new PacketProcessor(connection, rss, rfs, us, logger);
        }

        // Method used by client
        public ConnectionClasses(int sourcePort, int destinationPort, ILogger logger)
        {
            Connection connection = new Connection(sourcePort);
            rss = new ReliableSlowStream(logger);
            rfs = new ReliableFastStream(logger);
            us = new UnreliableStream(logger);
            pp  = new PacketProcessor(connection, destinationPort, rss, rfs, us, logger);
        }
    }
}