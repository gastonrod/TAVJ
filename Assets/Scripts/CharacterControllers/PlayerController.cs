using System.Net;
using System.Numerics;
using Connections.Streams;
using DefaultNamespace;
using UnityEditor.UI;
using UnityEngine;
using WorldManagement;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    private ReliableFastStream _reliableFastStream;
    private IPEndPoint _ipEndPoint;
    private static byte _playerId;
    private static ClientWorldController _worldController;
    private int _acumTimeFrames = 0;
    private int _msBetweenFrames = 100;
    private ParticleSystem bigExplosion;


    void Start()
    {
        bigExplosion = gameObject.GetComponentInChildren<ParticleSystem>();
        bigExplosion.Stop();
    }
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

        int deltaTimeInMs = (int)(1000 * Time.deltaTime);
        _acumTimeFrames += deltaTimeInMs;
        if (_acumTimeFrames > _msBetweenFrames)
        {
            UpdatePosition();
            _acumTimeFrames = _acumTimeFrames % _msBetweenFrames;
            byte input = InputUtils.GetKeyboardInput();
            if (input != 0)
            {
                _reliableFastStream.SendInput(input, _playerId, _ipEndPoint);
                if (InputUtils.PlayerMoved(input))
                {
                    _worldController.PredictMovePlayer(InputUtils.DecodeInput(input),
                        UnreliableStream.PACKET_SIZE);
                }
                if (InputUtils.PlayerAttacked(input))
                {
                    bigExplosion.Emit(20);
                }
            }
        }
    }

    private void UpdatePosition()
    {
        if (_worldController != null)
        {
            Vector3 playerPosition = _worldController.GetPlayerPosition() + Vector3.down;
            gameObject.transform.position = playerPosition;
            
        }
    }

    public static void SetClientData(byte playerId, ClientWorldController worldController)
    {
        _playerId = playerId;
        _worldController = worldController;
    }
}
