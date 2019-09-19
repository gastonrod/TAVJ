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
        public short serverPort;
        private Connection _connection;
        private GameObject _playerPrefab;
        
        public Game(GameObject playerPrefab)
        {
            _playerPrefab = playerPrefab;
        }
        
        public void Start()
        {
            Destroy(GameObject.FindGameObjectWithTag("Player"));
            
        }
    
        public void Update()
        {
           
        }
    }   

}