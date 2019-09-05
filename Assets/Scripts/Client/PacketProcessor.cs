using System;
using System.Collections;
using System.Collections.Generic;
using Streams;
using UnityEngine;

namespace Client
{
    public class PacketProcessor
    {
        private readonly Connection _connection;
        private readonly byte _clientId;
        private readonly IDictionary<byte, IStream> _streamDictionary;
        private byte _nextStreamId;
        
        public PacketProcessor(Connection connection, byte clientId)
        {
            _connection = connection;
            _clientId = clientId;
            _streamDictionary = new Dictionary<byte, IStream>();
            _nextStreamId = 0;
        }

        public void RegisterStream(IStream stream)
        {
            _streamDictionary.Add(_nextStreamId++, stream);
        }

        public void Update()
        {
            // Receive messages
            IList<byte[]> serializedMessages = _connection.ReceiveAllData();
            foreach (var serializedMessage in serializedMessages)
            {
                var message = PacketProcessorProtocol.Deserialize(serializedMessage);
                bool foundStream = _streamDictionary.TryGetValue(message.StreamId, out IStream stream);
                if (!foundStream)
                {
                    throw new Exception($"Received message for unknown stream {message.StreamId}");
                }
                stream.Give(message.Payload);
            }
            
            // Send messages
            foreach (var e in _streamDictionary)
            {
                byte streamId = e.Key;
                IStream stream = e.Value;
                IList<byte[]> payloadsToSend = stream.GetPendingMessagesForSend();
                foreach (byte[] payload in payloadsToSend)
                {
                    var message = new PacketProcessorProtocol.Message();
                    message.StreamId = streamId;
                    message.Payload = payload;
                    _connection.SendData(PacketProcessorProtocol.Serialize(message));
                }
            }
        }
    }
}
