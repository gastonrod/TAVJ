using System.Collections.Generic;
using System.Net;
using Connections;
using Connections.Loggers;
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
        _worldController = new ClientWorldController(new FramesStorer(),(ClientLogger)_logger);
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
            if (!_worldController.ClientSetPlayer())
            {
                ReceiveCharacterId();
            }
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
        byte[] snapshot = _worldController.GetNextFrame();
        if (snapshot == null)
        {
            return;
        }

        _worldController.SetPositions(snapshot);
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

    private void ReceiveCharacterId()
    {
        Queue<IPDataPacket> receivedData = ConnectionClasses.rss.GetReceivedData();
        while (receivedData.Count > 0)
        {
            byte[] msg = receivedData.Dequeue().message;
            byte charId = msg[0];
            PlayerController.SetClientData(charId, _worldController);
            _worldController.SpawnPlayer(charId, playerColor);
        }
    }
}
