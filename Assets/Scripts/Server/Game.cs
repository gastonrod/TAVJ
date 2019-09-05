using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.Object;

namespace Server
{
    public class Game
    {
        public bool enableServer;
        public String clientAddress;
        public short clientPort;
        public short serverPort;

        private IPAddress _clientAddress;
        private GameObject[] _cubes;
        private Connection _connection;
        private Protocol.Positions _positions;
    
        public void Start()
        {
            if (enableServer)
            {
                _clientAddress = IPAddress.Parse(clientAddress);
                _connection = new Connection(_clientAddress, serverPort, clientPort);
                _cubes = GameObject.FindGameObjectsWithTag("Cube");
                _positions = new Protocol.Positions();  
            }
        }
    
        public void Update()
        {
            if (enableServer)
            {
                Vector3 firstCube = _cubes[0].transform.position;
                _positions.x1 = firstCube.x;
                _positions.y1 = firstCube.y;
                _positions.z1 = firstCube.z;
            
                Vector3 secondCube = _cubes[1].transform.position;
                _positions.x2 = secondCube.x;
                _positions.y2 = secondCube.y;
                _positions.z2 = secondCube.z;
            
                _connection.SendData(Protocol.Serialize(_positions));
            }
        }
    }   

}