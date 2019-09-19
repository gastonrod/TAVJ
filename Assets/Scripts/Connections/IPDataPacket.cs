using System.Net;

namespace Connections
{
    public struct IPDataPacket
    {
        public IPEndPoint ip;
        public byte[] message;

        public IPDataPacket(IPEndPoint ip, byte[] message)
        {
            this.ip = ip;
            this.message = message;
        }
        public override string ToString()
        {
            return ip + " , " +  "[" + string.Join(",", message) + "]";
        }
    }
}