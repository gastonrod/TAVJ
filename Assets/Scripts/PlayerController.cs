using Streams;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IStream _stream;
    
    void Start()
    {
        
    }

    public void SetStream(IStream stream)
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
