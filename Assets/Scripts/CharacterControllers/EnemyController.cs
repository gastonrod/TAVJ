using System;
using UnityEngine;
using WorldManagement;

namespace DefaultNamespace
{
    public class EnemyController
    {
        
        private GameObject _player;
        private GameObject _me;
        private ServerWorldController _swc;
        private byte _id;

        private int _maxMove = 3;
        public EnemyController(ServerWorldController swc, GameObject me, byte id)
        {
            _player = GameObject.FindWithTag("Player");
            _swc = swc;
            _me = me;
            _id = id;
        }

        public void Update()
        {
            Debug.Log(_player.transform.position + " <> " + _me.transform.position);
            Vector3 move =  _player.transform.position - _me.transform.position;
            int x = Math.Abs(move.x) < Math.Abs(move.z) ? 0 : Math.Sign(move.x);
            int z = Math.Abs(move.z) < Math.Abs(move.x) ? 0 : Math.Sign(move.z);
            move.x = x;
            move.z = z;
            _swc.MovePlayer(_id, move);
        }

    }
}