using System;
using Protocols;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerMovementCalculator
    {
        private static float _speed = 5.0f;

        public static Vector3 GetVectorDirectionFromMovementDirection(MovementProtocol.Direction movementDirection)
        {
            switch (movementDirection)
            {
                case MovementProtocol.Direction.Up:
                    return Vector3.forward;
                case MovementProtocol.Direction.Down:
                    return Vector3.back;
                case MovementProtocol.Direction.Left:
                    return Vector3.left;
                case MovementProtocol.Direction.Right:
                    return Vector3.right;
                case MovementProtocol.Direction.Nop:
                    return Vector3.zero;
                default:
                    throw new Exception($"Unknown direction {movementDirection}");
            }
        }

        public static Vector3 GetVectorDirectionFromMovementDirectionAndRotation(MovementProtocol.Direction movementDirection, Vector3 forwardVector, Vector3 rightVector)
        {
            switch (movementDirection)
            {
                case MovementProtocol.Direction.Up:
                    return forwardVector;
                case MovementProtocol.Direction.Down:
                    return -forwardVector;
                case MovementProtocol.Direction.Left:
                    return -rightVector;
                case MovementProtocol.Direction.Right:
                    return rightVector;
                case MovementProtocol.Direction.Nop:
                    return Vector3.zero;
                default:
                    throw new Exception($"Unknown direction {movementDirection}");
            }
        }
        
        public static Vector3 CalculateDelta(Vector3 direction, float deltaTime)
        {
            return direction.normalized * (_speed * deltaTime);
        }
    }
}