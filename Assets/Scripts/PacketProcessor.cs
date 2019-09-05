using Streams;
using System.Collections.Generic;

namespace DefaultNamespace
{
    public class PacketProcessor
    {
        private Connection _connection;
        private IDictionary<int, IStream> _streamDictionary;
        private int nextStreamId;
        
        public PacketProcessor(Connection connection)
        {
            _connection = connection;
            _streamDictionary = new Dictionary<int, IStream>();
            nextStreamId = 0;
        }

        public void RegisterStream(IStream stream)
        {
            _streamDictionary.Add(nextStreamId++, stream);
        }

        public void Update()
        {
            foreach (var e in _streamDictionary)
            {
                
            }
        }
    }
}