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
        
        public ConnectionClasses(int sourcePort, int destinationPort, ILogger logger)
        {
            Connection connection = new Connection(sourcePort, 1);
            rss = new ReliableSlowStream(logger);
            rfs = new ReliableFastStream(logger);
            us = new UnreliableStream(logger);
            pp  = new PacketProcessor(connection, destinationPort, rss, rfs, us, logger);
        }
    }
}