using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Connection
{
    private readonly Queue<byte[]> _messages = new Queue<byte[]>();

    private readonly UdpClient _udpClient;

    public Connection(IPAddress destinationIpAddress, short sourcePort, short destinationPort)
    {
        _udpClient = new UdpClient(sourcePort);
        SetToIgnoreICMPPortUnreachable(_udpClient);
        _udpClient.Connect(destinationIpAddress, destinationPort);
        var remoteIpEndPoint = new IPEndPoint(destinationIpAddress, destinationPort);
        new Thread(() =>
        {
            while (true)
            {
                var newMessage = _udpClient.Receive(ref remoteIpEndPoint);
                lock (_messages)
                {
                    _messages.Enqueue(newMessage);
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
    
    public void SendData(byte[] data)
    {
        _udpClient.Send(data, data.Length);
    }

    public byte[] ReceiveData()
    {
        lock (_messages)
        {
            if (_messages.Count > 0)
                return _messages.Dequeue();
            return null;
        }
    }
}