﻿using System;
using System.Collections.Generic;
using System.Net;
using DefaultNamespace;
using Protocols;
using Streams;
using UnityEngine;
using static UnityEngine.Object;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Server
{
    public class Game
    {
        private short _serverPort;

        private readonly double _tickrate;
        
        private Connection _connection;
        private IStream<IPEndPoint> _joinStream;
        private PacketProcessor _packetProcessor;
        private SnapshotHandler _snapshotHandler;
        
        private Dictionary<byte, ClientInfo> _clientsInfo;
        private int _joinedPlayersCount = 0;
        private int killedPlayersCount = 0;

        private GameObject _playerPrefab;

        private byte _nextClientId;
        private static readonly int Color = Shader.PropertyToID("_Color");

        public Game(GameObject playerPrefab, short serverPort, double tickrate)
        {
            _playerPrefab = playerPrefab;
            _serverPort = serverPort;
            _clientsInfo = new Dictionary<byte, ClientInfo>();
            _tickrate = tickrate;
        }
        
        public void Start()
        {
            Debug.Log("ServerGame: Starting server...");
            Destroy(GameObject.FindGameObjectWithTag("Player"));
            Destroy(GameObject.FindGameObjectWithTag("CrossHair"));
            _connection = new Connection(_serverPort);
            _packetProcessor = new PacketProcessor(_connection);
            _joinStream = new ReliableSlowStream<IPEndPoint>();
            _packetProcessor.RegisterStream(JoinProtocol.JOIN_CLIENT_ID, _joinStream);
            _snapshotHandler = new SnapshotHandler(_tickrate);
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
                    JoinStream = reliableSlowStream,
                    Alive = true
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
                    GameObject newPlayer = Instantiate(_playerPrefab, new Vector3(0,1,0), Quaternion.identity);
                    newPlayer.GetComponent<Renderer>().material.SetColor(Color, UnityEngine.Color.green);
                    info.PlayerGameObject = newPlayer;
                    info.PlayerTransform = newPlayer.GetComponent<Transform>();
                    info.CharacterController = newPlayer.GetComponent<CharacterController>();
                    Debug.Log($"ServerGame: Received join accept message from client {clientId}");
                    break;
                }
            }
            
            if (_joinedPlayersCount == 0) return;

            // Serialize world
            _snapshotHandler.Update(_clientsInfo, _joinedPlayersCount, killedPlayersCount);
            _packetProcessor.Update();
        }

        public void FixedUpdate()
        {
            // Process inputs
            foreach (var clientInfo in _clientsInfo)
            {
                byte clientId = clientInfo.Key;
                ClientInfo info = clientInfo.Value;
                if (!info.Joined || !info.Alive) continue;
                IList<(byte[], IPEndPoint)> inputsWithMetadata = info.InputStream.ReceiveMessages();
                foreach (var inputWithMetadata in inputsWithMetadata)
                {
                    MovementProtocol.MovementMessage movementMessage = MovementProtocol.DeserializeMessage(inputWithMetadata.Item1);
                    int inputId = movementMessage.id;
                    MovementProtocol.Direction direction = movementMessage.direction;
                    Vector3 vectorDirection = PlayerMovementCalculator.GetVectorDirectionFromMovementDirectionAndRotation(direction, info.PlayerTransform.forward, info.PlayerTransform.right);
                    info.NextInputId = inputId >= info.NextInputId ? inputId + 1 : info.NextInputId;
                    Vector3 delta = PlayerMovementCalculator.CalculateDelta(vectorDirection, Time.fixedDeltaTime);
                    info.CharacterController.Move(delta);
                    info.PlayerTransform.rotation = new Quaternion(0f, movementMessage.horizontalRotation, 0f, movementMessage.scalarRotation);
                    byte killedPlayerId = movementMessage.killedPlayerId;
                    if (movementMessage.killedPlayerId != 0)
                    {
                        bool foundKilledPlayer = _clientsInfo.TryGetValue(killedPlayerId, out ClientInfo killedPlayerInfo);
                        if (!foundKilledPlayer)
                        {
                            throw new Exception($"Player {clientId} killed player {killedPlayerId}, but the latter's info was not found");
                        }

                        if (killedPlayerInfo.Joined && killedPlayerInfo.Alive)
                        {
                            Destroy(killedPlayerInfo.PlayerGameObject);
                            killedPlayerInfo.Alive = false;
                            killedPlayersCount++;
                        }
                    }
                }
            }
        }
    }
}