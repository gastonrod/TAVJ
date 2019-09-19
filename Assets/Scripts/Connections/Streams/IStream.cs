using System.Collections.Generic;
using System.Net;

namespace Connections.Streams
{
    public interface IStream
    {

        void SaveMessageToSend(byte[] message, IPEndPoint ip);
        Queue<IPDataPacket> GetMessageToSend();
        void SaveReceivedData(IPDataPacket data);
        Queue<IPDataPacket> GetReceivedData();
    }
}