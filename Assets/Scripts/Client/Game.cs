using System;
using System.Collections.Generic;
using System.Net;
using Protocols;
using Streams;
using UnityEngine;
using static UnityEngine.Object;

namespace Client
{
    public class Game
    {
        public String serverAddress;
        public short serverPort;
        public short clientPort;

        private IPAddress _serverAddress;
        private Connection _connection;
        private GameObject[] _cubes;
        private PacketProcessor _packetProcessor;
        private GameObject _playerGameObject;
        private PlayerController _playerController;

        private UnreliableStream _unreliableStream;
        private ReliableFastStream _reliableFastStream;
        private ReliableSlowStream _reliableSlowStream;
        
        private enum State {START, JOIN_REQUESTED, JOINED}
        private State _state;
        private byte _clientId;

        private GameObject _playerPrefab;
        private Dictionary<byte, Transform> players;
        
        public Game(GameObject playerPrefab)
        {
            _playerPrefab = playerPrefab;
            _state = State.START;
            players = new Dictionary<byte, Transform>();
        }
        
        public void Start()
        {
            _serverAddress = IPAddress.Parse(serverAddress);
            _connection = new Connection(_serverAddress, clientPort, serverPort);
            _packetProcessor = new PacketProcessor(_connection);
            _playerGameObject = GameObject.FindGameObjectWithTag("Player");
            _playerController = _playerGameObject.GetComponent<PlayerController>();
            _unreliableStream = new UnreliableStream();
            _reliableFastStream = new ReliableFastStream();
            _reliableSlowStream = new ReliableSlowStream();
            _packetProcessor.RegisterStream(_unreliableStream);
            _packetProcessor.RegisterStream(_reliableFastStream);
            _packetProcessor.RegisterStream(_reliableSlowStream);
            _reliableSlowStream.SendMessage(JoinProtocol.SerializeJoinRequestMessage(new JoinRequestMessage()));
            _state = State.JOIN_REQUESTED;
        }
    
        public void Update()
        {
            _playerController.Update();
            _packetProcessor.Update();
            switch (_state)
            {
                case State.START:
                    break;
                case State.JOIN_REQUESTED:
                    // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                    JoinRequestedUpdate();
                    break;
                case State.JOINED:
                    // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                    JoinedUpdate();
                    break;
                default:
                    throw new Exception("I'm not able to handle this, sorry :c");
            }
        }

        private void JoinRequestedUpdate()
        {
            IList<byte[]> messages = _reliableSlowStream.ReceiveMessages();
            foreach (var message in messages)
            {
                var joinResponseMessage = JoinProtocol.DeserializeJoinAcceptMessage(message);
                _clientId = joinResponseMessage.ClientId;
                _packetProcessor.SetClientId(_clientId);
                _reliableSlowStream.SendMessage(JoinProtocol.SerializeJoinAcceptMessage(new JoinAcceptMessage {ClientId = _clientId}));
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                players.Add(_clientId, _playerGameObject.GetComponent<Transform>());
                _state = State.JOINED;
                _playerController.SetStream(_reliableFastStream);
                break;
            }
        }

        private void JoinedUpdate()
        {
            IList<byte[]> messages = _unreliableStream.ReceiveMessages();
            foreach (var message in messages)
            {
                var snapshot = GameProtocol.DeserializeSnapshotMessage(message);
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                ApplySnapshot(snapshot);
            }
        }

        private void ApplySnapshot(GameProtocol.SnapshotMessage snapshotMessage)
        {
            foreach (var currentPlayerInfo in snapshotMessage.PlayersInfo)
            {
                bool foundCurrentPlayer = players.TryGetValue(currentPlayerInfo.ClientId, out Transform currentPlayerTransform);
                if (foundCurrentPlayer)
                {
                    currentPlayerTransform.SetPositionAndRotation(currentPlayerInfo.Position, currentPlayerInfo.Rotation);
                }
                else
                {
                    GameObject currentPlayerGameObject = Instantiate(_playerPrefab, currentPlayerInfo.Position, currentPlayerInfo.Rotation);
                    // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                    players.Add(currentPlayerInfo.ClientId, currentPlayerGameObject.GetComponent<Transform>());
                }
            }
        }
    }

}