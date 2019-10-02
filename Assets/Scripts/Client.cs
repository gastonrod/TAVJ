using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Connections;
using Connections.Loggers;
using Connections.Streams;
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
    public static byte playerId;
    // Server IP+port
    public static IPEndPoint ipEndPoint;

    private GameObject[] _gameObjects = new GameObject[byte.MaxValue];
    private ILogger _logger = new ClientLogger();
    private Vector3 offset = new Vector3(1,0,1);
    private Color playerColor = Color.red;
    private FramesStorer _framesStorer;
    
    public int frameRate = 60;
    public int messageRate = 10;
    private int _msBetweenMessages;
    private int _msBetweenFrames;
    private int _acumTimeMessages;
    private int _acumTimeFrames;
    
    void Start()
    {
        _framesStorer = new FramesStorer();
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
            if (!_gameObjects[playerId])
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
        for (int j = 1; j < snapshot.Length; j++)
        {
            int i = snapshot[j];
            if (!_gameObjects[i])
            {
                SpawnObject(snapshot, j);
                j += UnreliableStream.PACKET_SIZE;
            }
            else
            {
                j+=2;
                _gameObjects[i].transform.position = Utils.ByteArrayToVector3(snapshot, j) + offset;
                j += 12;
            }
        }
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

    private void SpawnObject(byte[] message, int j)
    {
        byte id = message[j++];
        PrimitiveType primitiveType = (PrimitiveType)message[j++];
        GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
        Vector3 pos = Utils.ByteArrayToVector3(message, j);
        gameObject.transform.position = pos;
        _gameObjects[id] = gameObject;
    }

    private void ReceiveCharacterId()
    {
        Queue<IPDataPacket> receivedData = _connectionClasses.rss.GetReceivedData();
        while (receivedData.Count > 0)
        {
            byte[] msg = receivedData.Dequeue().message;
            playerId = msg[0];
            SpawnCharacter();
        }
    }

    private void SpawnCharacter()
    {
         GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
         Vector3 pos = new Vector3(5, 0, 0) + offset;
         capsule.transform.position = pos;
         capsule.GetComponent<Renderer>().material.color = playerColor;
         _gameObjects[playerId] = capsule;
    }
}
