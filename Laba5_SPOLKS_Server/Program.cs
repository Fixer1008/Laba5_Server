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
        private static void Receive()
        {
            const string EndMessage = "<End>";

            UdpClient receivingUdpClient = new UdpClient(11000);

            IPAddress ipAddress = IPAddress.Parse("192.168.0.103");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 5001);

            IPEndPoint remoteEndPoint = null;

            receivingUdpClient.Connect(ipEndPoint);

            try
            {
                while (true)
                {
                    var receivedDatagram = receivingUdpClient.Receive(ref remoteEndPoint);
                    Console.WriteLine("Client: {0}", Encoding.UTF8.GetString(receivedDatagram));

                    if (Encoding.UTF8.GetString(receivedDatagram) == EndMessage)
                    {
                        break;
                    }

                    Console.Write("Server: ");
                    var sendMessage = Console.ReadLine();
                    var sendBytes = receivingUdpClient.Send(Encoding.UTF8.GetBytes(sendMessage), sendMessage.Length);

                    if (sendMessage == EndMessage)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {                
                throw e;
            }
            finally
            {
                receivingUdpClient.Close();
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Waiting...");
            Receive();
            Console.WriteLine("Please, ");
            Console.ReadLine();
        }
    }
}
