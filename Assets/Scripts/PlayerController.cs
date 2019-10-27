using System.Net;
using JetBrains.Annotations;
using Streams;
using UnityEngine;

public class PlayerController
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
            Debug.Log("PlayerController: Sending down direction");
            _stream.SendMessage(Protocols.MovementProtocol.Serialize(Protocols.MovementProtocol.Direction.Down));
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            Debug.Log("PlayerController: Sending up direction");
            _stream.SendMessage(Protocols.MovementProtocol.Serialize(Protocols.MovementProtocol.Direction.Up));
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            Debug.Log("PlayerController: Sending left direction");
            _stream.SendMessage(Protocols.MovementProtocol.Serialize(Protocols.MovementProtocol.Direction.Left));
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            Debug.Log("PlayerController: Sending right direction");
            _stream.SendMessage(Protocols.MovementProtocol.Serialize(Protocols.MovementProtocol.Direction.Right));
        }
    }
}
