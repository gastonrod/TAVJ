using System.Collections.Generic;
using UnityEngine;
using static Protocols.ReliableStreamProtocol;

namespace Streams
{
    public class ReliableFastStream<T> : IStream<T>
    {
        private static readonly byte DEFAULT_ID = 1;

        private int _nextMessageId = 1;
        private SeenManager _seenManager = new SeenManager();
        
        private List<byte[]> _sendList = new List<byte[]>();
        private IList<(byte[], T)> _receiveList = new List<(byte[], T)>();

        public void SendMessage(byte[] data)
        {
            _sendList.Add(SerializeMessage(new DataMessage(_nextMessageId++, data)));
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
            foreach (var serializedMessageToSend in _sendList)
            {
                LightMessage lightMessage = DeserializeLightMessage(serializedMessageToSend);
                if (lightMessage.Type == MessageType.DATA)
                {
                    _sendList.Add(serializedMessageToSend);
                }
            }
            return result;
        }

        public void Give(byte[] data, T metadata)
        {
            IMessage message = DeserializeMessage(data);
            if (message.GetMessageType() == MessageType.DATA)
            {
                _sendList.Add(SerializeMessage(new AckMessage(message.MessageId)));
                if (_seenManager.GiveMessage(message.MessageId))
                {
                    _receiveList.Add((((DataMessage) message).Payload, metadata));
                }
            }
            else
            {
                _sendList.RemoveAll((serializedMessage) =>
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

        private class SeenManager
        {
            private int _highestConsecutiveAck = 0;
            private SortedSet<int> _nonConsecutiveAcks = new SortedSet<int>();
            
            /* Tells this SeenManager that a message with id messageId has arrived
             * Returns whether the message has been seen before or not
             */
            public bool GiveMessage(int messageId)
            {
                if (messageId <= _highestConsecutiveAck || _nonConsecutiveAcks.Add(messageId))
                {
                    return true;
                }
                var enumerator = _nonConsecutiveAcks.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == _highestConsecutiveAck + 1)
                    {
                        _highestConsecutiveAck++;
                    }
                    else
                    {
                        break;
                    }
                }
                enumerator.Dispose();
                _nonConsecutiveAcks.RemoveWhere(item => item <= _highestConsecutiveAck);
                return false;
            }
        }
    }
}