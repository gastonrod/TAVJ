using Streams;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IStream _stream;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetStream(IStream stream)
    {
        _stream = stream;
    }
    
    // Update is called once per frame
    void Update()
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
