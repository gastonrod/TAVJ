﻿using System.Collections.Generic;
using System.Net;
using Connections;
using Connections.Loggers;
using DefaultNamespace;
using UnityEngine;
using WorldManagement;
using ILogger = Connections.Loggers.ILogger;

public class Server : MonoBehaviour
{
    // Start is called before the first frame update
    private ConnectionClasses _connectionClasses;
    public int sourcePort = 9696;
    private ILogger _logger = new ServerLogger();
    private List<IPEndPoint> connectedClients = new List<IPEndPoint>();

    private ServerWorldController _worldController;
    private byte snapshotId = 0;
    public int frameRate = 3;
    public int delayInMs = 50;
    public int packetLossPct = 5;

    // How many messages per second.
    public int messageRate = 10;
    private int _msBetweenMessages;
    private int _msBetweenFrames;
    private int _messagesAcumTime;
    private int _framesAcumTime;

    
    void Start()
    {
        _worldController = new ServerWorldController();
        _msBetweenMessages = 1000 / messageRate;
        _msBetweenFrames = 1000 / frameRate;
        _connectionClasses = Utils.GetConnectionClasses(sourcePort, delayInMs, packetLossPct,_logger);
    }
    
    void Update()
    {
        int deltaTimeInMs = (int)(1000 * Time.deltaTime);
        _messagesAcumTime += deltaTimeInMs;
        _framesAcumTime += deltaTimeInMs;
        if (_messagesAcumTime > _msBetweenMessages)
        {
            _messagesAcumTime = _messagesAcumTime % _msBetweenMessages;
            _connectionClasses.pp.Update();
            ListenNewConnections();
            ListenInputs();
            SendPositions();
        }
        if (_framesAcumTime > _msBetweenFrames)
        {
            _framesAcumTime = _framesAcumTime % _msBetweenFrames;
            _worldController.Update();
        }
    }

    private void SendPositions()
    {
        byte[] positions = _worldController.GetPositions(snapshotId++);
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
            _worldController.MovePlayer(msg[0], InputUtils.DecodeInput(msg[1]));
            if (InputUtils.InputSpawnEnemy(msg[1]))
            {
                _worldController.SpawnEnemy();
            }
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
                _connectionClasses.rss.SpawnPlayer(_worldController.SpawnCharacter(), _worldController.GetMovementSpeed(), ipDataPacket.ip);
            }
        }
    }
}
