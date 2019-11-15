using System;
using UnityEngine;

namespace Client
{
    public class ClientIdHolder : MonoBehaviour
    {
        private byte _clientId;
        
        public void SetClientId(byte clientId)
        {
            _clientId = clientId;
        }

        public byte GetClientId()
        {
            return _clientId;
        }
        
        void Start() {}
        
        void Update() {}

    }
}