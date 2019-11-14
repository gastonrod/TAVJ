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
        private Transform _playerTransform;
        
        public PlayerController(Queue<MovementProtocol.MovementMessage> inputQueue, Transform playerTransform)
        {
            _nextInputId = 0;
            _inputQueue = inputQueue;
            _playerTransform = playerTransform;
        }
        
        public void SetStream(IStream<IPEndPoint> stream)
        {
            _stream = stream;
        }
    
        public void Update()
        {
            if (_stream == null) return;
            MovementProtocol.Direction direction = MovementProtocol.Direction.Nop;

            if (Input.GetKey(KeyCode.S))
            {
                direction = MovementProtocol.Direction.Down;
            }
            else if (Input.GetKey(KeyCode.W))
            {
                direction = MovementProtocol.Direction.Up;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                direction = MovementProtocol.Direction.Left;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                direction = MovementProtocol.Direction.Right;
            }
            
            _playerTransform.Rotate(new Vector3(0 , Input.GetAxis("Mouse X") * 3,0));
            var rotation = _playerTransform.rotation;
            float horizontalRotation = rotation.y;
            float scalarRotation = rotation.w;

            byte killedPlayerId = 0;
            if (Input.GetKey(KeyCode.Space))
            {
                killedPlayerId = 1;
            }
            
            MovementProtocol.MovementMessage message = new MovementProtocol.MovementMessage
            {
                id = _nextInputId++,
                direction = direction,
                horizontalRotation = horizontalRotation,
                scalarRotation = scalarRotation,
                killedPlayerId = killedPlayerId
            };
            _stream.SendMessage(MovementProtocol.SerializeMessage(message));
            _inputQueue.Enqueue(message);
        }
    }
}