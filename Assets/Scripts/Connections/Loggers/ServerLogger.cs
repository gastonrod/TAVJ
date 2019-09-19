using UnityEngine;

namespace Connections.Loggers
{
    public class ServerLogger : ILogger
    {
        public void Log(string s)
        {
            Debug.Log("SERVER:: " + s);
        }
    }
}