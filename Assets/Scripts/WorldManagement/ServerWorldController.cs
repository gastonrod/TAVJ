using System.Collections.Generic;
using System.Linq;
using Connections.Loggers;
using Connections.Streams;
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
        private int _spawnRate;
        private Random _random = new Random();
        private Material _enemiesMaterial;
        private Dictionary<byte, EnemyController> enemies = new Dictionary<byte, EnemyController>();
        private Queue<byte> enemiesToDestroy = new Queue<byte>();

        public ServerWorldController(int spawnRate, ServerLogger logger) : base(logger)
        {
            Vector3 planeScale = GameObject.FindWithTag("Platform").transform.localScale*5;
            _xRange = (int)(planeScale.x * 2);
            _zRange = (int)(planeScale.z * 2);
            _spawnRate = spawnRate;
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

        private bool spawnEnemy = true;
        public void Update()
        {
            foreach (KeyValuePair<byte, EnemyController> pair in enemies)
            {
                pair.Value.Update();
            }
//            if(spawnEnemy)
//                SpawnEnemy();
            spawnEnemy = !spawnEnemy;
        }

        public void PlayerAttacked(byte playerId)
        {
//            DeleteAllNPCs();
//            enemies = new Dictionary<byte, EnemyController>();
            HashSet<byte> enemiesToDelete = AttackNPCsNearPoint(_gameObjects[playerId].transform.position);
            foreach (byte id in enemiesToDelete)
            {
                enemies.Remove(id);
                enemiesToDestroy.Enqueue(id);
            }
        }

        public void MoveEnemy(byte id, Vector3 move, byte playerId)
        {
            Vector3 playerPos = _gameObjects[playerId].transform.position;
            MovePlayer(id, move);
            if (_gameObjects[id].transform.position.Equals(playerPos))
            {
                AttackPlayer(playerId);
            }
        }

        private void AttackPlayer(byte playerId)
        {
            DestroyGameObject(playerId);
            enemiesToDestroy.Enqueue(playerId);
        }

        public Queue<byte> ObjectsToDestroy()
        {
            Queue<byte> toReturn = enemiesToDestroy;
            enemiesToDestroy = new Queue<byte>();
            return toReturn;
        }
    }
}