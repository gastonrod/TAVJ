using System;
using System.Collections.Generic;
using UnityEngine;
using static Protocols.ReliableStreamProtocol;

namespace Streams
{
    public class ReliableSlowStream<T> : IStream<T>
    {
        public static readonly byte DEFAULT_ID = 2;
        
        private int _nextMessageId = 1;
        private const int _millisThresholdForResend = 1000;
        private SeenManager _seenManager = new SeenManager();
        
        private List<SerializedMessageAndTickCount> _sendDataList = new List<SerializedMessageAndTickCount>();
        private List<byte[]> _sendAckList = new List<byte[]>();
        private IList<(byte[], T)> _receiveList = new List<(byte[], T)>();
        private bool _ack = true;
        
        public ReliableSlowStream()
        {
            
        }

        public ReliableSlowStream(int seenMessages)
        {
            for (int i = 1; i <= seenMessages; i++)
            {
                _seenManager.GiveMessage(i);
                _sendAckList.Add(SerializeMessage(new AckMessage(_nextMessageId)));
            }
        }
        
        public ReliableSlowStream(bool ack)
        {
            _ack = false;
        }
        
        public void SendMessage(byte[] data)
        {
            _sendDataList.Add(new SerializedMessageAndTickCount(SerializeMessage(new DataMessage(_nextMessageId++, data)), 0));
        }
        
        public IList<(byte[], T)> ReceiveMessages()
        {
            var result = _receiveList;
            _receiveList = new List<(byte[], T)>();
            return result;
        }

        public IList<byte[]> GetPendingMessagesForSend()
        {
            int currentTickCount = Environment.TickCount;
            var allMessages = new List<byte[]>(_sendDataList.Count + _sendAckList.Count);
            foreach (var messageAndTick in _sendDataList)
            {
                byte[] serializedMessage = messageAndTick.SerializedMessage;
                int tickCount = messageAndTick.TickCount;
                if (tickCount == 0 || tickCount + _millisThresholdForResend < currentTickCount)
                {
                    allMessages.Add(serializedMessage);
                    messageAndTick.TickCount = currentTickCount;
                }
            }
            allMessages.AddRange(_sendAckList);
            _sendAckList = new List<byte[]>();
            return allMessages;
        }

        public void Give(byte[] data, T metadata)
        {
            IMessage message = DeserializeMessage(data);
            if (message.GetMessageType() == MessageType.DATA)
            {
                if (_ack) _sendAckList.Add(SerializeMessage(new AckMessage(message.MessageId)));
                if (_seenManager.GiveMessage(message.MessageId))
                {
                    _receiveList.Add((((DataMessage) message).Payload, metadata));
                }
            }
            else
            {
                _sendDataList.RemoveAll(serializedMessageAndTickCount =>
                {
                    LightMessage lightMessage = DeserializeLightMessage(serializedMessageAndTickCount.SerializedMessage);
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
            _sendDataList = new List<SerializedMessageAndTickCount>();
            _sendAckList = new List<byte[]>();
            _receiveList = new List<(byte[], T)>();
        }

        private class SerializedMessageAndTickCount
        {
            public byte[] SerializedMessage;
            public int TickCount;

            public SerializedMessageAndTickCount(byte[] serializedMessage, int tickCount)
            {
                SerializedMessage = serializedMessage;
                TickCount = tickCount;
            }
        }
    }
}