using System.Collections.Generic;

namespace Streams
{
    public class ReliableFastStream : IStream
    {
        private IList<byte[]> _sendList = new List<byte[]>();
        private IList<byte[]> _receiveList = new List<byte[]>();

        public void SendMessage(byte[] data)
        {
            _sendList.Add(data);
        }
        
        public IList<byte[]> ReceiveMessages()
        {
            if (_receiveList.Count == 0) return null;
            var result = _receiveList;
            _receiveList = new List<byte[]>();
            return result;
        }

        public IList<byte[]> GetPendingMessagesForSend()
        {
            if (_sendList.Count == 0) return null;
            var result = _sendList;
            _sendList = new List<byte[]>();
            return result;
        }

        public void Give(byte[] data)
        {
            _receiveList.Add(data);
        }
    }
}