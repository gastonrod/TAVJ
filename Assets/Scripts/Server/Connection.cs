using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public class Connection
    {
        private readonly Queue<(IPEndPoint, byte[])> _messages = new Queue<(IPEndPoint, byte[])>();
        private readonly UdpClient _udpClient;

        public Connection(short sourcePort)
        {
            _udpClient = new UdpClient(sourcePort);
            SetToIgnoreICMPPortUnreachable(_udpClient);
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            new Thread(() =>
            {
                while (true)
                {
                    var newMessage = _udpClient.Receive(ref remoteIpEndPoint);
                    lock (_messages)
                    {
                        _messages.Enqueue((remoteIpEndPoint, newMessage));
                    }
                }
            }).Start();
        }

        private void SetToIgnoreICMPPortUnreachable(UdpClient client)
        {
            uint IOC_IN = 0x80000000;     
            uint IOC_VENDOR = 0x18000000;     
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;     
            client.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
        }
    
        public void SendData(byte[] data, IPEndPoint ipEndPoint)
        {
            _udpClient.Send(data, data.Length, ipEndPoint);
        }

        public (IPEndPoint, byte[]) ReceiveData()
        {
            lock (_messages)
            {
                if (_messages.Count > 0)
                    return _messages.Dequeue();
                return (null, null);
            }
        }

        public IList<(IPEndPoint, byte[])> ReceiveAllData()
        {
            lock (_messages)
            {
                IList<(IPEndPoint, byte[])> dataList = new List<(IPEndPoint, byte[])>(_messages.Count);
                while (_messages.Count > 0)
                {
                    dataList.Add(_messages.Dequeue());
                }

                return dataList;
            }
        }
    }
}