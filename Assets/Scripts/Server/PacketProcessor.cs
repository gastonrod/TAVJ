using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Protocols;
using Streams;
using UnityEngine;

namespace Server
{
    public class PacketProcessor
    {
        private readonly Connection _connection;
        private readonly Dictionary<byte, IPEndPoint> _clientEndpointDictionary;
        private readonly Dictionary<byte, Dictionary<byte, IStream<IPEndPoint>>> _clientStreamsDictionary;

        public PacketProcessor(Connection connection)
        {
            _connection = connection;
            _clientEndpointDictionary = new Dictionary<byte, IPEndPoint>();
            _clientStreamsDictionary = new Dictionary<byte, Dictionary<byte, IStream<IPEndPoint>>>();
        }
        
        public void RegisterClient(byte clientId, IPEndPoint ipEndPoint)
        {
            // TODO: Check if not present already
            _clientEndpointDictionary.Add(clientId, ipEndPoint);
        }

        public void RegisterStream(byte clientId, params IStream<IPEndPoint>[] streams)
        {
            bool foundStreamsDictionary = _clientStreamsDictionary.TryGetValue(clientId, out Dictionary<byte, IStream<IPEndPoint>> streamsDictionary);
            if (!foundStreamsDictionary)
            {
                streamsDictionary = new Dictionary<byte, IStream<IPEndPoint>>();
                _clientStreamsDictionary.Add(clientId, streamsDictionary);
            }
            foreach (var stream in streams)
            {
                streamsDictionary.Add(stream.GetId(), stream);
            }
        }
        
        public void Update()
        {
            // Receive messages
            IList<(IPEndPoint, byte[])> serializedMessagesWithEndpoint = _connection.ReceiveAllData();
            foreach (var serializedMessageWithEndpoint in serializedMessagesWithEndpoint)
            {
                var serializedMessage = serializedMessageWithEndpoint.Item2;
                var endpoint = serializedMessageWithEndpoint.Item1;
                var message = PacketProcessorProtocol.DeserializeClientToServerMessage(serializedMessage);
                bool foundClient = _clientStreamsDictionary.TryGetValue(message.ClientId, out Dictionary<byte, IStream<IPEndPoint>> streamsDictionary);
                if (!foundClient)
                {
                    throw new Exception("Did not find client");
                }
                bool foundStream = streamsDictionary.TryGetValue(message.StreamId, out IStream<IPEndPoint> stream);
                if (!foundStream)
                {
                    throw new Exception("Did not find stream");
                }
                stream.Give(message.Payload, endpoint);
            }
            
            // Send messages
            foreach (var clientStreams in _clientStreamsDictionary)
            {
                var clientId = clientStreams.Key;
                IPEndPoint endpoint = null;
                var streams = clientStreams.Value;
                foreach (var streamKV in streams)
                {
                    if (clientId != 0)
                    {
                        var stream = streamKV.Value;
                        IList<byte[]> messagesToSend = stream.GetPendingMessagesForSend();
                        foreach (var messageToSend in messagesToSend)
                        {
                            if (endpoint == null)
                            {
                                bool foundEndpoint = _clientEndpointDictionary.TryGetValue(clientId, out endpoint);
                                if (!foundEndpoint)
                                {
                                    throw new Exception($"Did not find endpoint for client {clientId}");
                                }
                            }
                            var serverToClientMessage = new PacketProcessorProtocol.ServerToClientMessage()
                                {StreamId = stream.GetId(), Payload = messageToSend};
                            _connection.SendData(PacketProcessorProtocol.SerializeServerToClientMessage(serverToClientMessage), endpoint);
                        }
                    }
                }
            }
        }
    }
}