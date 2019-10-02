﻿using System.Collections.Generic;
using System.Net;
using Connections;
using Connections.Loggers;
using Connections.Streams;
using UnityEngine;
using ILogger = Connections.Loggers.ILogger;

public class Server : MonoBehaviour
{
    // Start is called before the first frame update
    private ConnectionClasses _connectionClasses;
    public int sourcePort = 9696;
    private ILogger _logger = new ServerLogger();
    private List<IPEndPoint> connectedClients = new List<IPEndPoint>();
    
    private GameObject[] gameObjects = new GameObject[byte.MaxValue];
    private byte gameObjectsCount = 0;
    private byte[] gameObjectTypes = new byte[byte.MaxValue];
    private byte lastGameObjectID = 0;
    private byte snapshotID = 0;

    // How many messages per second.
    public int messageRate = 10;
    private int _msBetweenMessages;
    private int _acumTime;

    public int movementSpeed = 1;
    
    void Start()
    {
        _msBetweenMessages = 1000 / messageRate;
        _connectionClasses = Utils.GetConnectionClasses(sourcePort, _logger);
    }
    
    void Update()
    {
        int deltaTimeInMs = (int)(1000 * Time.deltaTime);
        _acumTime += deltaTimeInMs;
        if (_acumTime > _msBetweenMessages)
        {
            _acumTime = _acumTime % _msBetweenMessages;
            _connectionClasses.pp.Update();
            ListenNewConnections();
            ListenInputs();
            SendPositions();
        }
    }

    private void SendPositions()
    {
        byte[] positions = new byte[gameObjectsCount * UnreliableStream.PACKET_SIZE + 1];
        positions[0] = snapshotID++;
        for (int i = 0, j = 1; i < gameObjects.Length; i++)
        {
            if (!gameObjects[i])
            {
                continue;
            }
            positions[j++] = (byte)i;
            positions[j++] = gameObjectTypes[i];
            Utils.Vector3ToByteArray(gameObjects[i].transform.position, positions, j);
            j += 12;
        }
        foreach(IPEndPoint clientIp in connectedClients)
        {
            _connectionClasses.us.SaveMessageToSend(positions, clientIp);
        }
    }
    private void ListenInputs()
    {
        Queue<IPDataPacket> queue = _connectionClasses.rfs.GetReceivedData();
        while (queue.Count > 0)
        {
            IPDataPacket ipDataPacket = queue.Dequeue();
            byte[] msg = ipDataPacket.message;
            gameObjects[msg[0]].transform.position += Utils.DecodeInput(msg[1]) * movementSpeed;
        }
    }

    private void ListenNewConnections()
    {
        Queue<IPDataPacket> queue = _connectionClasses.rss.GetReceivedData();
        while (queue.Count > 0)
        {
            IPDataPacket ipDataPacket = queue.Dequeue();
            connectedClients.Add(ipDataPacket.ip);
            byte[] msg = ipDataPacket.message;
            if (msg != null && msg.Length > 0)
            {
                SpawnCharacter();
                _connectionClasses.rss.SpawnPlayer(lastGameObjectID++, ipDataPacket.ip);
            }
        }
    }

    private void SpawnCharacter()
    {
         GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
         Vector3 pos = new Vector3(5, 0, 0);
         capsule.transform.position = pos;
         gameObjects[lastGameObjectID] = capsule;
         gameObjectTypes[lastGameObjectID] = (byte)PrimitiveType.Capsule;
         gameObjectsCount++;
    }
}