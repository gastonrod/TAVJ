using System;
using UnityEditor;
using UnityEngine;
using WorldManagement;

namespace DefaultNamespace
{
    public class EnemyController
    {
        
        private GameObject[] _players;
        private GameObject _me;
        private ServerWorldController _swc;
        private byte _id;

        private int _maxMove = 3;
        public EnemyController(ServerWorldController swc, GameObject me, byte id)
        {
            _players = GameObject.FindGameObjectsWithTag("Player");
            _swc = swc;
            _me = me;
            _id = id;
        }

        public void Update()
        {
            GameObject closestPlayer = null;
            byte closestPlayerId = 0;
            float minDist = int.MaxValue;
            for (int i = 0; i < _players.Length; i++)
            {
                float dist = Math.Abs((_me.transform.position - _players[i].transform.position).magnitude);
                if (dist < minDist)
                {
                    closestPlayer = _players[i];
                    closestPlayerId = (byte)i;
                    minDist = dist;
                }
            }
            Vector3 move =  closestPlayer.transform.position - _me.transform.position;
            int x = Math.Abs(move.x) < Math.Abs(move.z) ? 0 : Math.Sign(move.x);
            int z = Math.Abs(move.z) < Math.Abs(move.x) ? 0 : Math.Sign(move.z);
            move.x = x;
            move.z = z;
            _swc.MoveEnemy(_id, move, closestPlayerId);
            _players = GameObject.FindGameObjectsWithTag("Player");
        }

    }
}