using System;
using System.Net;
using Streams;
using UnityEngine;

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
        private PlayerController _playerController;
    
        public void Start()
        {
            _serverAddress = IPAddress.Parse(serverAddress);
            _connection = new Connection(_serverAddress, clientPort, serverPort);
            _packetProcessor = new PacketProcessor(_connection);
            _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            IStream stream = new UnreliableStream();
            _packetProcessor.RegisterStream(stream);
            _playerController.SetStream(stream);
        }
    
        public void Update()
        {
        
        }
    }

}