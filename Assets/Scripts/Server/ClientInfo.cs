using System.Net;
using Streams;
using UnityEngine;

namespace Server
{
    public class ClientInfo
    {
        public bool Joined;
        public GameObject PlayerGameObject;
        public Transform PlayerTransform;
        public CharacterController CharacterController;
        public IStream<IPEndPoint> SnapshotStream;
        public IStream<IPEndPoint> InputStream;
        public IStream<IPEndPoint> JoinStream;
        public int NextInputId = 0;
        public bool Alive;
    }
}