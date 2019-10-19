using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Connections;
using Connections.Loggers;
using Connections.Streams;
using DefaultNamespace;
using Interpolation;
using UnityEngine;
using ILogger = Connections.Loggers.ILogger;

public class Client : MonoBehaviour
{
    public static ConnectionClasses _connectionClasses;
    // Server IP
    public string ipAddressString = "127.0.1.1";
    public int sourcePort = 6969;
    // Server port
    public int destinationPort = 9696;
    public byte clientId = 77;
    // Server IP+port
    public static IPEndPoint ipEndPoint;

    private GameObject[] _gameObjects = new GameObject[byte.MaxValue];
    private ILogger _logger = new ClientLogger();
    private Vector3 offset = new Vector3(1,0,1);
    private Color playerColor = Color.red;
    private FramesStorer _framesStorer;
    private WorldController _worldController;
    
    public int frameRate = 60;
    public int messageRate = 10;
    private int _msBetweenMessages;
    private int _msBetweenFrames;
    private int _acumTimeMessages;
    private int _acumTimeFrames;
    
    void Start()
    {
        _framesStorer = new FramesStorer();
        _worldController = new WorldController(offset, _framesStorer);
        ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddressString), destinationPort);
        _connectionClasses = Utils.GetConnectionClasses(sourcePort, destinationPort, _logger);
        _connectionClasses.rss.InitConnection(clientId, ipEndPoint);
        _msBetweenMessages = 1000 / messageRate;
        _msBetweenFrames = 1000 / frameRate;
    }

    void Update()
    {
        int deltaTimeInMs = (int)(1000 * Time.deltaTime);
        _acumTimeMessages += deltaTimeInMs;
        _acumTimeFrames += deltaTimeInMs;

        if (_acumTimeMessages > _msBetweenMessages)
        {
            _acumTimeMessages = _acumTimeMessages % _msBetweenMessages;
            _connectionClasses.pp.Update();
            if (!_worldController.ClientSetPlayer())
            {
                ReceiveCharacterId();
            }
            ReceivePositions();
        }
        if (_acumTimeFrames > _msBetweenFrames)
        {
            _acumTimeFrames = _acumTimeFrames % _msBetweenFrames;
            UpdatePositions();
        }
    }

    private void UpdatePositions()
    {
        byte[] snapshot = _framesStorer.GetNextFrame();
        if (snapshot == null)
        {
            return;
        }

        _worldController.SetPositions(snapshot);
    }

    private void ReceivePositions()
    {
        Queue<IPDataPacket> receivedData = _connectionClasses.us.GetReceivedData();
        if (receivedData.Count > 0)
        {
            byte[] message = receivedData.Dequeue().message;
            if (message != null && message.Length > 0)
            {
                _framesStorer.StoreFrame(message);
            }
        }
    }

    private void ReceiveCharacterId()
    {
        Queue<IPDataPacket> receivedData = _connectionClasses.rss.GetReceivedData();
        while (receivedData.Count > 0)
        {
            byte[] msg = receivedData.Dequeue().message;
            byte charId = msg[0];
            PlayerController.SetClientData(charId, _worldController);
            _worldController.SpawnClientPlayer(charId, playerColor);
            _framesStorer.SetCharId(charId);
        }
    }
}
