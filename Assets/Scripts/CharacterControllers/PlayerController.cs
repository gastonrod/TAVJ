using System.Net;
using Connections.Streams;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UIElements;
using WorldManagement;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    private ReliableFastStream _reliableFastStream;
    private IPEndPoint _ipEndPoint;
    private static byte _playerId;
    private static ClientWorldController _worldController;
    private int _acumTimeFrames = 0;
    private int _msBetweenFrames = 1000;
    private ParticleSystem _bigExplosion;
    private byte _lastInputId = 1;
    private byte _input;


    void Start()
    {
        _bigExplosion = gameObject.GetComponentInChildren<ParticleSystem>();
        _bigExplosion.Stop();
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
        byte keyboardInput = InputUtils.GetKeyboardInput();
        if (InputUtils.PlayerAttacked(keyboardInput))
        {
            UpdatePosition();
            _reliableFastStream.SendInput(new InputPackage(_lastInputId, (byte)InputCodifications.HIT_ENEMIES), _playerId, _ipEndPoint);
            _bigExplosion.Emit(20);
            _worldController.PlayerAttacked();
        }
        if (_acumTimeFrames > _msBetweenFrames)
        {
            _acumTimeFrames = _acumTimeFrames % _msBetweenFrames;
            byte input = keyboardInput == 0 ? _input : keyboardInput;
            if (input != 0)
            {
                InputPackage inputPackage = new InputPackage(_lastInputId++, input);
                _reliableFastStream.SendInput(inputPackage, _playerId, _ipEndPoint);
                if (InputUtils.PlayerMoved(input))
                {
                    _worldController.PredictMovePlayer(inputPackage,
                        UnreliableStream.PACKET_SIZE);
                }
            }
            _input = 0;
        }
        else
        {
            _input = keyboardInput != 0 ? keyboardInput : _input;
        }
    }

    // Move the position of this game object so that the splash effect appears below the player
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
