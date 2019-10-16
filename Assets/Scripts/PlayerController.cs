using System.Net;
using Connections;
using Connections.Streams;
using DefaultNamespace;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private ReliableFastStream _reliableFastStream;
    private IPEndPoint _ipEndPoint;
    private static byte _playerId;
    private static WorldController _worldController;
    
    
    void Update()
    {
        if (_reliableFastStream == null)
        {
            _reliableFastStream = Client._connectionClasses.rfs;
        }

        if (_ipEndPoint == null)
        {
            _ipEndPoint = Client.ipEndPoint;
        }
        byte input = 0;
        if (Input.GetKeyDown(KeyCode.A))
        {
            input |= (byte)InputCodifications.LEFT;
        } 
        if (Input.GetKeyDown(KeyCode.D))
        {
            input |= (byte)InputCodifications.RIGHT;
        } 
        if (Input.GetKeyDown(KeyCode.W))
        {
            input |= (byte)InputCodifications.UP;
        } 
        if (Input.GetKeyDown(KeyCode.S))
        {
            input |= (byte)InputCodifications.DOWN;
        }

        if (input != 0)
        {
            _reliableFastStream.SendInput(input, _playerId, _ipEndPoint);
            _worldController.PredictMovePlayer(_playerId, Utils.DecodeInput(input), UnreliableStream.PACKET_SIZE);
        }
    }

    public static void SetClientData(byte playerId, WorldController worldController)
    {
        _playerId = playerId;
        _worldController = worldController;
    }
}
