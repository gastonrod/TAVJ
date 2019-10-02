using System.Net;
using Connections.Streams;
using DefaultNamespace;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private ReliableFastStream _reliableFastStream;
    private IPEndPoint _ipEndPoint;
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
            _reliableFastStream.SendInput(input, Client.playerId, _ipEndPoint);
        }
    }
}
