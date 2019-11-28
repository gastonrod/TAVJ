using System.Collections.Generic;
using System.Net;
using Connections;
using Connections.Loggers;
using Connections.Streams;
using DefaultNamespace;
using WorldManagement;
using UnityEngine;
using ILogger = Connections.Loggers.ILogger;

public class Client : MonoBehaviour
{
    public static ConnectionClasses ConnectionClasses;
    // Server IP
    public string ipAddressString = "127.0.1.1";
    public int sourcePort = 6969;
    // Server port
    public int destinationPort = 9696;
    public byte clientId = 77;
    // Server IP+port
    public static IPEndPoint ipEndPoint;
    public int delayInMs = 50;

    private ILogger _logger = new ClientLogger();
    public Color playerColor = Color.red;
    private ClientWorldController _worldController;
    
    public int frameRate = 60;
    public int messageRate = 10;
    private int _msBetweenMessages;
    private int _msBetweenFrames;
    private int _acumTimeMessages;
    private int _acumTimeFrames;
    
    void Start()
    {
<<<<<<< Updated upstream
        _worldController = new ClientWorldController(new FramesStorer(frameRate),(ClientLogger)_logger);
=======
        _worldController = new ClientWorldController(new FramesStorer(),(ClientLogger)_logger);
>>>>>>> Stashed changes
        ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddressString), destinationPort);
        ConnectionClasses = Utils.GetConnectionClasses(sourcePort, delayInMs, 0,_logger);
        ConnectionClasses.rss.InitConnection(clientId, ipEndPoint);
        _msBetweenMessages = 1000 / messageRate;
        _msBetweenFrames = 1000 / frameRate;
    }

    void Update()
    {
        int deltaTimeInMs = (int)(1000 * Time.deltaTime);
        _acumTimeMessages += deltaTimeInMs;
        _acumTimeFrames += deltaTimeInMs;

        // Send & receive messages
        if (_acumTimeMessages > _msBetweenMessages)
        {
            _acumTimeMessages = _acumTimeMessages % _msBetweenMessages;
            ConnectionClasses.pp.Update();
            ReceiveFromRSS();
            ReceivePositions();
        }
        // Update current frame
        if (_acumTimeFrames > _msBetweenFrames)
        {
            _acumTimeFrames = _acumTimeFrames % _msBetweenFrames;
            UpdatePositions();
        }
    }

    private void UpdatePositions()
    {
        _worldController.UpdatePositions();
    }

    private void ReceivePositions()
    {
        Queue<IPDataPacket> receivedData = ConnectionClasses.us.GetReceivedData();
        while (receivedData.Count > 0)
        {
            byte[] message = receivedData.Dequeue().message;
            if (message != null && message.Length > 0)
            {
                _worldController.StoreFrame(message);
            }
        }
    }

    private void ReceiveFromRSS()
    {
        Queue<IPDataPacket> receivedData = ConnectionClasses.rss.GetReceivedData();
        if (receivedData.Count > 0)
        {
            byte[] msg = receivedData.Dequeue().message;
            byte msgType = msg[0];
            byte charId = msg[1];
            switch (msgType)
            {
                case (byte)RSSPacketTypes.SPAWNED_PLAYER:
                    PlayerController.SetClientData(charId, _worldController);
                    _worldController.SpawnPlayer(charId, playerColor);
                    break;
                case (byte)RSSPacketTypes.DESTROY_OBJECT:
                    _worldController.DestroyObject(charId, msg[2] == (byte) PrimitiveType.Capsule);
                    break;
                case (byte)RSSPacketTypes.CREATE_OBJECT:
                    _worldController.CreateObject(charId, (PrimitiveType)msg[2]);
                    break;
            }
        }
    }
}
