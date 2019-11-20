using System.Collections.Generic;
using static Protocols.ReliableStreamProtocol;

namespace Streams
{
    public class ReliableFastStream<T> : IStream<T>
    {
        private static readonly byte DEFAULT_ID = 1;

        private int _nextMessageId = 1;
        private SeenManager _seenManager = new SeenManager();
        
        private List<byte[]> _sendDataList = new List<byte[]>();
        private List<byte[]> _sendAckList = new List<byte[]>();
        private IList<(byte[], T)> _receiveList = new List<(byte[], T)>();

        public void SendMessage(byte[] data)
        {
            _sendDataList.Add(SerializeMessage(new DataMessage(_nextMessageId++, data)));
        }
        
        public IList<(byte[], T)> ReceiveMessages()
        {
            var result = _receiveList;
            _receiveList = new List<(byte[], T)>();
            return result;
        }

        public IList<byte[]> GetPendingMessagesForSend()
        {
            var allMessages = new List<byte[]>(_sendDataList.Count + _sendAckList.Count);
            allMessages.AddRange(_sendDataList);
            allMessages.AddRange(_sendAckList);
            _sendAckList = new List<byte[]>();
            return allMessages;
        }

        public void Give(byte[] data, T metadata)
        {
            IMessage message = DeserializeMessage(data);
            if (message.GetMessageType() == MessageType.DATA)
            {
                _sendAckList.Add(SerializeMessage(new AckMessage(message.MessageId)));
                if (_seenManager.GiveMessage(message.MessageId))
                {
                    _receiveList.Add((((DataMessage) message).Payload, metadata));
                }
            }
            else
            {
                _sendDataList.RemoveAll(serializedMessage =>
                {
                    LightMessage lightMessage = DeserializeLightMessage(serializedMessage);
                    return lightMessage.Type == MessageType.DATA && lightMessage.MessageId == message.MessageId;
                });
            }
        }

        public byte GetId()
        {
            return DEFAULT_ID;
        }

        public void Reset()
        {
            _nextMessageId = 1;
            _seenManager = new SeenManager();
            _sendDataList = new List<byte[]>();
            _sendAckList = new List<byte[]>();
            _receiveList = new List<(byte[], T)>();
        }
    }
}