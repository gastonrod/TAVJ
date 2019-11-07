using System.Collections.Generic;
using System.Net;
using Protocols;
using Streams;
using UnityEngine;

namespace Client
{
    public class PlayerController
    {
        private int _nextInputId;
        private Queue<MovementProtocol.MovementMessage> _inputQueue;
        private IStream<IPEndPoint> _stream;

        public PlayerController(Queue<MovementProtocol.MovementMessage> inputQueue)
        {
            _nextInputId = 0;
            _inputQueue = inputQueue;
        }
        
        public void SetStream(IStream<IPEndPoint> stream)
        {
            _stream = stream;
        }
    
        public void Update()
        {
            if (_stream == null) return;
            MovementProtocol.Direction direction = MovementProtocol.Direction.Up;
            bool gotInput = true;
            
            if (Input.GetKey(KeyCode.DownArrow))
            {
                direction = MovementProtocol.Direction.Down;
            }
            else if (Input.GetKey(KeyCode.UpArrow))
            {
                direction = MovementProtocol.Direction.Up;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                direction = MovementProtocol.Direction.Left;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                direction = MovementProtocol.Direction.Right;
            }
            else
            {
                gotInput = false;
            }

            if (gotInput)
            {
                MovementProtocol.MovementMessage message = new MovementProtocol.MovementMessage
                {
                    id = _nextInputId++,
                    direction = direction
                };
                _stream.SendMessage(MovementProtocol.SerializeMessage(message));
                _inputQueue.Enqueue(message);
            }
        }
    }
}