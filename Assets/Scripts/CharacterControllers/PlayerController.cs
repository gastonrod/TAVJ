using System.Net;
using Connections.Streams;
using DefaultNamespace;
using UnityEngine;
using WorldManagement;

public class PlayerController : MonoBehaviour
{
    private ReliableFastStream _reliableFastStream;
    private IPEndPoint _ipEndPoint;
    private static byte _playerId;
    private static ClientWorldController _worldController;

    
    void Update()
    {
        if (_reliableFastStream == null)
        {
            _reliableFastStream = Client.ConnectionClasses.rfs;
        }

        if (_ipEndPoint == null)
        {
            _ipEndPoint = Client.ipEndPoint;
        }

        byte input = InputUtils.GetKeyboardInput();
        if (input != 0)
        {
            _reliableFastStream.SendInput(input, _playerId, _ipEndPoint);
            _worldController.PredictMovePlayer(_playerId, InputUtils.DecodeInput(input), UnreliableStream.PACKET_SIZE);
        }
    }

    public static void SetClientData(byte playerId, ClientWorldController worldController)
    {
        _playerId = playerId;
        _worldController = worldController;
    }
}
