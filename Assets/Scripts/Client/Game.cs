using System;
using System.Collections.Generic;
using System.Net;
using Protocols;
using Streams;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Client
{
    public class Game
    {
        private String _stringServerAddress;
        private short _serverPort;
        private short _clientPort;

        private IPAddress _serverAddress;
        private Connection _connection;
        private GameObject[] _cubes;
        private PacketProcessor _packetProcessor;
        private GameObject _playerGameObject;
        private PlayerController _playerController;

        private UnreliableStream<IPEndPoint> _unreliableStream;
        private ReliableFastStream<IPEndPoint> _reliableFastStream;
        private ReliableSlowStream<IPEndPoint> _reliableSlowStream;
        
        private enum State {START, JOIN_REQUESTED, JOINED}
        private State _state;
        private byte _clientId;
        
        private Dictionary<byte, Transform> players;

        private readonly SnapshotHandler _snapshotHandler;
        private static readonly int Color = Shader.PropertyToID("_Color");

        public Game(GameObject playerPrefab, String serverAddress, short serverPort, short clientPort, double tickrate)
        {
            _stringServerAddress = serverAddress;
            _serverPort = serverPort;
            _clientPort = clientPort;
            _state = State.START;
            players = new Dictionary<byte, Transform>();
            _snapshotHandler = new SnapshotHandler(playerPrefab, players, tickrate);
        }
        
        public void Start()
        {
            Debug.Log("ClientGame: Starting client");
            _serverAddress = IPAddress.Parse(_stringServerAddress);
            _connection = new Connection(_serverAddress, _clientPort, _serverPort);
            _packetProcessor = new PacketProcessor(_connection);
            _playerGameObject = GameObject.FindGameObjectWithTag("Player");
            _playerController = new PlayerController();
            _unreliableStream = new UnreliableStream<IPEndPoint>();
            _reliableFastStream = new ReliableFastStream<IPEndPoint>();
            _reliableSlowStream = new ReliableSlowStream<IPEndPoint>();
            _packetProcessor.RegisterStream(_unreliableStream);
            _packetProcessor.RegisterStream(_reliableFastStream);
            _packetProcessor.RegisterStream(_reliableSlowStream);
            RequestJoin();
            _state = State.JOIN_REQUESTED;
        }
    
        public void Update()
        {
            _packetProcessor.Update();
            switch (_state)
            {
                case State.START:
                    break;
                case State.JOIN_REQUESTED:
                    JoinRequestedUpdate();
                    break;
                case State.JOINED:
                    JoinedUpdate();
                    break;
                default:
                    throw new Exception("I'm not able to handle this, sorry :c");
            }
        }

        private void RequestJoin()
        {
            _reliableSlowStream.SendMessage(JoinProtocol.SerializeJoinRequestMessage(new JoinRequestMessage()));
            Debug.Log($"ClientGame: join requested");
        }
        
        private void JoinRequestedUpdate()
        {
            IList<(byte[], IPEndPoint)> messagesWithMetadata = _reliableSlowStream.ReceiveMessages();
            foreach (var messageWithMetadata in messagesWithMetadata)
            {
                var message = messageWithMetadata.Item1;
                var joinResponseMessage = JoinProtocol.DeserializeJoinAcceptMessage(message);
                _clientId = joinResponseMessage.ClientId;
                Debug.Log($"ClientGame: received join response message with client ID {_clientId}");
                _packetProcessor.SetClientId(_clientId);
                _reliableSlowStream.SendMessage(JoinProtocol.SerializeJoinAcceptMessage(new JoinAcceptMessage {ClientId = _clientId}));
                Debug.Log($"ClientGame: sent join accept message with client ID {_clientId}");
                players.Add(_clientId, _playerGameObject.GetComponent<Transform>());
                _playerGameObject.GetComponent<Renderer>().material.SetColor(Color, UnityEngine.Color.red);
                _state = State.JOINED;
                _playerController.SetStream(_reliableFastStream);
                break;
            }
        }

        private void JoinedUpdate()
        {
            IList<(byte[], IPEndPoint)> messagesWithMetadata = _unreliableStream.ReceiveMessages();
            foreach (var messageWithMetadata in messagesWithMetadata)
            {
                var message = messageWithMetadata.Item1;
                var snapshot = GameProtocol.DeserializeSnapshotMessage(message);
                _snapshotHandler.AddSnapshot(snapshot);
            }
            _snapshotHandler.Update();
        }

        public void FixedUpdate()
        {
            _playerController.Update();
        }

    }

}