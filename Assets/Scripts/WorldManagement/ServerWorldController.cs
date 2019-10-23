using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Random = System.Random;

namespace WorldManagement
{
    public class ServerWorldController : WorldController
    {
        private byte _lastGameObjectId = 0;
        private int _xRange;
        private int _zRange;
        private Random _random = new Random();
        private Material _enemiesMaterial;
        private Random rand = new Random();
        private Dictionary<byte, EnemyController> enemies = new Dictionary<byte, EnemyController>();

        public ServerWorldController()
        {
            Vector3 planeScale = GameObject.FindWithTag("Platform").transform.localScale*5;
            _xRange = (int)(planeScale.x * 2);
            _zRange = (int)(planeScale.z * 2);
        }

        public byte SpawnCharacter()
        {
            SpawnCharacter(_lastGameObjectId, Color.white);
            return _lastGameObjectId++;
        }

        public byte GetMovementSpeed()
        {
            return _movementSpeed;
        }

        public void MovePlayer(byte id, Vector3 movement)
        {
            _gameObjects[id].transform.position += movement * _movementSpeed;
        }

        public void SpawnEnemy()
        {
            Vector3 pos = new Vector3(_random.Next(_xRange) - _xRange/2 , 1.1f, _random.Next(_zRange) - _zRange/2);
            GameObject enemy = SpawnObject(_lastGameObjectId, PrimitiveType.Cylinder, pos, Color.cyan);
            enemies[_lastGameObjectId] = new EnemyController(this, enemy, _lastGameObjectId);
            _lastGameObjectId++;
        }

        public byte[] GetPositions(byte id)
        {
            return base.GetPositions(id);
        }

        public void Update()
        {
            foreach (KeyValuePair<byte, EnemyController> pair in enemies)
            {
                pair.Value.Update();
            }
        }
    }
}