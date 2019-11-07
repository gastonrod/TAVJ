using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerMovementCalculator
    {
        private static float _speed = 5.0f;
        
        public static Vector3 CalculateDelta(Vector3 direction, float deltaTime)
        {
            return direction.normalized * (_speed * deltaTime);
        }
    }
}