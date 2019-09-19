using UnityEngine;

namespace Connections.Loggers
{
    public class ClientLogger : ILogger
    {
        public void Log(string s)
        {
            Debug.Log("CLIENT:: " + s);
        }
    }
}