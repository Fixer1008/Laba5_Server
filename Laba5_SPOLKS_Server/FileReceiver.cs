using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Diagnostics;

namespace Laba5_SPOLKS_Server
{
    public class FileReceiver
    {
        private const int Size = 8192;
        private const string SyncMessage = "SYNC";
        private const string ClientIp = "192.168.0.104";

        private readonly UdpFileClient _udpFileReceiver;
        private readonly UdpFileClient _udpFileSender;

        private FileStream _fileStream;
        private IPEndPoint _remoteIpEndPoint = null;

        public FileDetails FileDetails { get; set; }

        public FileReceiver()
        {
            _udpFileReceiver = new UdpFileClient(11000);
            _udpFileSender = new UdpFileClient();
        }

        void InitializeUdpClients()
        {
            _udpFileSender.Client.SendTimeout = _udpFileReceiver.Client.ReceiveTimeout = 10000;
        }

        public int Receive()
        {
            InitializeUdpClients();
            ReceiveFileDetails();

            Console.WriteLine(FileDetails.FileName);
            Console.WriteLine(FileDetails.FileLength);

            ReceiveFileData();

            return 0;
        }

        private int ReceiveFileDetails()
        {
            MemoryStream memoryStream = new MemoryStream();

            try
            {
                var receivedFileInfo = _udpFileReceiver.Receive(ref _remoteIpEndPoint);

                XmlSerializer serializer = new XmlSerializer(typeof(FileDetails));

                memoryStream.Write(receivedFileInfo, 0, receivedFileInfo.Length);
                memoryStream.Position = 0;

                FileDetails = (FileDetails)serializer.Deserialize(memoryStream);
                return 0;
            }
            catch (Exception e)
            {
                _udpFileReceiver.Close();
                Console.WriteLine(e.Message);                
            }
            finally
            {
                memoryStream.Dispose();
            }

            return -1;
        }

        private int ReceiveFileData()
        {
            int filePointer = 0;

            try
            {
                if (FileDetails.FileLength > 0)
                {
                    _fileStream = new FileStream(FileDetails.FileName, FileMode.Append, FileAccess.Write);

                    IPAddress clientIpAddress = IPAddress.Parse(ClientIp);
                    IPEndPoint clientEndPoint = new IPEndPoint(clientIpAddress, 5000);

                    _udpFileSender.Connect(clientEndPoint);

                    for (_fileStream.Position = 0; _fileStream.Position < FileDetails.FileLength; )
                    {
                        var fileDataArray = _udpFileReceiver.Receive(ref _remoteIpEndPoint);

                        filePointer += fileDataArray.Length;
                        Console.WriteLine(filePointer);

                        _fileStream.Write(fileDataArray, 0, fileDataArray.Length);

                        var sendBytesAmount = _udpFileSender.Send(Encoding.UTF8.GetBytes(SyncMessage), SyncMessage.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
            finally
            {
                _udpFileReceiver.Close();
                _fileStream.Close();
                _fileStream.Dispose();
            }

            return 0;
        }
    }
}
