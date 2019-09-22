using System.Collections.Generic;

namespace Streams
{
    public class UnreliableStream<T> : IStream<T>
    {
        public static readonly byte DEFAULT_ID = 0;
        
        public UnreliableStream() {}

        private IList<byte[]> _sendList = new List<byte[]>();
        private IList<(byte[], T)> _receiveList = new List<(byte[], T)>();

        public void SendMessage(byte[] data)
        {
            _sendList.Add(data);
        }
        
        public IList<(byte[], T)> ReceiveMessages()
        {
            var result = _receiveList;
            _receiveList = new List<(byte[], T)>();
            return result;
        }

        public IList<byte[]> GetPendingMessagesForSend()
        {
            var result = _sendList;
            _sendList = new List<byte[]>();
            return result;
        }

        public void Give(byte[] data, T metadata)
        {
            _receiveList.Add((data, metadata));
        }

        public byte GetId()
        {
            return DEFAULT_ID;
        }
    }
}