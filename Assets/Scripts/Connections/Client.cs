using System;
using System.Collections.Generic;
using System.Net;
using Connections;
using Connections.Loggers;
using Connections.Streams;
using UnityEngine;
using ILogger = Connections.Loggers.ILogger;

public class Client : MonoBehaviour
{
    public static ConnectionClasses _connectionClasses;
    // Server IP
    public static readonly string ipAddressString = "127.0.1.1";
    public static readonly int sourcePort = 6969;
    // Server port
    public static readonly int destinationPort = 9696;
    public static readonly byte clientId = 77;
    public static byte playerId;
    // Server IP+port
    public static readonly IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddressString), destinationPort);

    private GameObject[] _gameObjects = new GameObject[byte.MaxValue];
    private byte currentSnapshotID = 0;
    private byte[][] _snapshots = new byte[4][];
    private ILogger _logger = new ClientLogger();
    private Vector3 offset = new Vector3(1,0,1);
    private Color playerColor = Color.red; 
    
    public int messageRate = 10;
    private int _msBetweenMessages;
    private int _acumTime;
    
    void Start()
    {
        _connectionClasses = Utils.GetConnectionClasses(sourcePort, destinationPort, _logger);
        _connectionClasses.rss.InitConnection(clientId, ipEndPoint);
        _msBetweenMessages = 1000 / messageRate;
        _connectionClasses = Utils.GetConnectionClasses(sourcePort, destinationPort, _logger);
    }

    void Update()
    {
        int deltaTimeInMs = (int)(1000 * Time.deltaTime);
        _acumTime += deltaTimeInMs;
        if (_acumTime > _msBetweenMessages)
        {
            _connectionClasses.pp.Update();
            if (!_gameObjects[playerId])
            {
                ReceiveCharacterId();
            }
            ReceivePositions();
        }
    }

    private void ReceivePositions()
    {
        Queue<IPDataPacket> receivedData = _connectionClasses.us.GetReceivedData();
        if (receivedData.Count > 0)
        {
            byte[] message = receivedData.Dequeue().message;
            byte snapshotID = message[0];
            if (snapshotID <= currentSnapshotID && Math.Abs(snapshotID-currentSnapshotID) < 10)
            {
                _logger.Log("discarding snapshot.");
                return;
            }
            currentSnapshotID = snapshotID;
            _logger.Log("Snapshot ID:" + snapshotID);
            for (int i = 0; i < _gameObjects.Length; i++)
            {
                int j = i * UnreliableStream.PACKET_SIZE+1;
                if (!_gameObjects[i])
                {
                    bool isEmpty = true;
                    for (; j < UnreliableStream.PACKET_SIZE; j++)
                    {
                        if (message[j] != 0)
                        {
                            isEmpty = false;
                            break;
                        }
                    }

                    if (!isEmpty)
                    {
                        SpawnObject(message, i);
                    }
                }
                else
                {
                    // Skip over item id and item type, only important when spawning the item.
                    j+=2;
                    _gameObjects[i].transform.position = Utils.ByteArrayToVector3(message, j) + offset;
                }
            }
        }
    }

    private void SpawnObject(byte[] message, int i)
    {
        byte id = (byte)i++;
        PrimitiveType primitiveType = (PrimitiveType)message[i++];
        GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
        Vector3 pos = Utils.ByteArrayToVector3(message, i);
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
