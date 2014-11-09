using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Laba5_SPOLKS_Server
{
    class Program
    {
        private static string Receive()
        {
            UdpClient receivingUdpClient = new UdpClient(11000);

            IPEndPoint remoteEndPoint = null;

            try
            {
                var receivedDatagram = receivingUdpClient.Receive(ref remoteEndPoint);
                receivingUdpClient.Close();
                return Encoding.UTF8.GetString(receivedDatagram);
            }
            catch (Exception e)
            {                
                throw e;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Waiting for message...");
            Console.WriteLine("Received datagram: {0}", Receive());
            Console.ReadLine();
        }
    }
}
