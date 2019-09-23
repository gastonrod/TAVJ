using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using Protocols;
using Streams;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.Object;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Server
{
    public class Game
    {
        private short _serverPort;
        
        private Connection _connection;
        private IStream<IPEndPoint> _joinStream;
        private PacketProcessor _packetProcessor;

        private Dictionary<byte, ClientInfo> _clientsInfo;
        private int _joinedPlayersCount = 0;

        private GameObject _playerPrefab;

        private byte _nextClientId;

        public Game(GameObject playerPrefab, short serverPort)
        {
            _playerPrefab = playerPrefab;
            _serverPort = serverPort;
            _clientsInfo = new Dictionary<byte, ClientInfo>();
        }
        
        public void Start()
        {
            Debug.Log("ServerGame: Starting server...");
            Destroy(GameObject.FindGameObjectWithTag("Player"));
            _connection = new Connection(_serverPort);
            _packetProcessor = new PacketProcessor(_connection);
            _joinStream = new ReliableSlowStream<IPEndPoint>();
            _packetProcessor.RegisterStream(JoinProtocol.JOIN_CLIENT_ID, _joinStream);
            _nextClientId = (byte) (JoinProtocol.JOIN_CLIENT_ID + 1);
        }

        public void Update()
        {
            _packetProcessor.Update();
            
            // Process join requests
            IList<(byte[], IPEndPoint)> serializedJoinRequestMessagesWithMetadata = _joinStream.ReceiveMessages();
            foreach (var serializedJoinRequestMessageWithMetadata in serializedJoinRequestMessagesWithMetadata)
            {
                JoinRequestMessage joinRequestMessage = JoinProtocol.DeserializeJoinRequestMessage(serializedJoinRequestMessageWithMetadata.Item1);
                IPEndPoint endpoint = serializedJoinRequestMessageWithMetadata.Item2;
                byte clientId = _nextClientId++;
                Debug.Log($"ServerGame: Received join request from: {endpoint}\tAssigning client ID {clientId}");
                _packetProcessor.RegisterClient(clientId, endpoint);
                UnreliableStream<IPEndPoint> unreliableStream = new UnreliableStream<IPEndPoint>();
                ReliableFastStream<IPEndPoint> reliableFastStream = new ReliableFastStream<IPEndPoint>();
                ReliableSlowStream<IPEndPoint> reliableSlowStream = new ReliableSlowStream<IPEndPoint>();
                _packetProcessor.RegisterStream(clientId, unreliableStream, reliableFastStream, reliableSlowStream);
                var newClientInfo = new ClientInfo
                {
                    Joined = false,
                    SnapshotStream = unreliableStream,
                    InputStream = reliableFastStream,
                    JoinStream = reliableSlowStream
                };
                _clientsInfo.Add(clientId, newClientInfo);
                newClientInfo.JoinStream.SendMessage(JoinProtocol.SerializeJoinResponseMessage(new JoinResponseMessage() {ClientId = clientId}));
                Debug.Log($"ServerGame: Sent client response message for client ID {clientId}");
            }

            // Process join accepts
            foreach (var clientInfo in _clientsInfo)
            {
                byte clientId = clientInfo.Key;
                ClientInfo info = clientInfo.Value;
                if (info.Joined) continue;
                IList<(byte[], IPEndPoint)> joinAcceptsWithMetadata = info.JoinStream.ReceiveMessages();
                foreach (var joinAcceptWithMetadata in joinAcceptsWithMetadata)
                {
                    JoinAcceptMessage joinAcceptMessage =
                        JoinProtocol.DeserializeJoinAcceptMessage(joinAcceptWithMetadata.Item1);
                    info.Joined = true;
                    _joinedPlayersCount++;
                    GameObject newPlayer = Instantiate(_playerPrefab, new Vector3(0,0,0), Quaternion.identity);
                    info.PlayerTransform = newPlayer.GetComponent<Transform>();
                    Debug.Log($"ServerGame: Received join accept message from client {clientId}");
                    break;
                }
            }
            
            if (_joinedPlayersCount == 0) return;
            
            // Process inputs
            foreach (var clientInfo in _clientsInfo)
            {
                byte clientId = clientInfo.Key;
                ClientInfo info = clientInfo.Value;
                if (!info.Joined) continue;
                IList<(byte[], IPEndPoint)> inputsWithMetadata = info.InputStream.ReceiveMessages();
                foreach (var inputWithMetadata in inputsWithMetadata)
                {
                    MovementProtocol.Direction direction = MovementProtocol.Deserialize(inputWithMetadata.Item1);
                    Vector3 delta;
                    switch (direction)
                    {
                        case MovementProtocol.Direction.Up:
                            Debug.Log($"Received UP direction input for client {clientId}");
                            delta = Vector3.forward;
                            break;
                        case MovementProtocol.Direction.Down:
                            Debug.Log($"Received DOWN direction input for client {clientId}");
                            delta = Vector3.back;
                            break;
                        case MovementProtocol.Direction.Left:
                            Debug.Log($"Received LEFT direction input for client {clientId}");
                            delta = Vector3.left;
                            break;
                        case MovementProtocol.Direction.Right:
                            Debug.Log($"Received RIGHT direction input for client {clientId}");
                            delta = Vector3.right;
                            break;
                        default:
                            throw new Exception("Unknown direction");
                    }
                    var position = info.PlayerTransform.position;
                    position.Set(position.x + delta.x,
                        position.y + delta.y,
                        position.z + delta.z);
                    info.PlayerTransform.SetPositionAndRotation(position, Quaternion.identity);
                }
            }

            // Serialize world
            GameProtocol.SnapshotMessage snapshotMessage = new GameProtocol.SnapshotMessage() {PlayersInfo = new GameProtocol.SnapshotMessage.SinglePlayerInfo[_joinedPlayersCount]};
            int currentPlayerCount = 0;
            foreach (var clientInfo in _clientsInfo)
            {
                byte clientId = clientInfo.Key;
                ClientInfo info = clientInfo.Value;
                if (!info.Joined) continue;
                var playerInfo = new GameProtocol.SnapshotMessage.SinglePlayerInfo {ClientId = clientId};
                var transform = info.PlayerTransform;
                playerInfo.Position = transform.position;
                playerInfo.Rotation = transform.rotation;
                snapshotMessage.PlayersInfo[currentPlayerCount++] = playerInfo;
            }
            
            // Broadcast serialized world
            foreach (var clientInfo in _clientsInfo)
            {
                byte clientId = clientInfo.Key;
                ClientInfo info = clientInfo.Value;
                if (!info.Joined) continue;
                info.SnapshotStream.SendMessage(GameProtocol.SerializeSnapshotMessage(snapshotMessage));
            }
            
            _packetProcessor.Update();
        }
        
        private class ClientInfo
        {
            public bool Joined;
            public Transform PlayerTransform;
            public IStream<IPEndPoint> SnapshotStream;
            public IStream<IPEndPoint> InputStream;
            public IStream<IPEndPoint> JoinStream;
        }
    }

}