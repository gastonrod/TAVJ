using System.Net;
using JetBrains.Annotations;
using Streams;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IStream<IPEndPoint> _stream;
    
    void Start()
    {
        
    }

    public void SetStream(IStream<IPEndPoint> stream)
    {
        _stream = stream;
    }
    
    public void Update()
    {
        if (_stream == null) return;
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _stream.SendMessage(Protocols.MovementProtocol.Serialize(Protocols.MovementProtocol.Direction.Down));
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            _stream.SendMessage(Protocols.MovementProtocol.Serialize(Protocols.MovementProtocol.Direction.Up));
        }
    }
}
