using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Protocols;
using Streams;
using UnityEngine;

namespace Client
{
    public class PacketProcessor
    {
        private readonly Connection _connection;
        private byte _clientId;
        private readonly IDictionary<byte, IStream<IPEndPoint>> _streamDictionary;

        public PacketProcessor(Connection connection)
        {
            _connection = connection;
            _streamDictionary = new Dictionary<byte, IStream<IPEndPoint>>();
        }

        public void RegisterStream(IStream<IPEndPoint> stream)
        {
            _streamDictionary.Add(stream.GetId(), stream);
        }

        public void SetClientId(byte clientId)
        {
            _clientId = clientId;
        }
        
        public void Update()
        {
            // Receive messages
            IList<byte[]> serializedMessages = _connection.ReceiveAllData();
            foreach (var serializedMessage in serializedMessages)
            {
                var message = PacketProcessorProtocol.DeserializeServerToClientMessage(serializedMessage);
                bool foundStream = _streamDictionary.TryGetValue(message.StreamId, out IStream<IPEndPoint> stream);
                if (!foundStream)
                {
                    throw new Exception($"Received message for unknown stream {message.StreamId}");
                }
                stream.Give(message.Payload, null);
            }
            
            // Send messages
            foreach (var e in _streamDictionary)
            {
                byte streamId = e.Key;
                IStream<IPEndPoint> stream = e.Value;
                IList<byte[]> payloadsToSend = stream.GetPendingMessagesForSend();
                foreach (byte[] payload in payloadsToSend)
                {
                    var message = new PacketProcessorProtocol.ClientToServerMessage
                    {
                        StreamId = streamId,
                        ClientId = _clientId,
                        Payload = payload
                    };
                    _connection.SendData(PacketProcessorProtocol.SerializeClientToServerMessage(message));
                }
            }
        }
    }
}
