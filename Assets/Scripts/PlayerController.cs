using System.Collections;
using System.Collections.Generic;
using Protocols;
using Streams;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IStream _stream;
    
    // Start is called before the first frame update
    void Start()
    {
        _stream = UnreliableStream.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {
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
