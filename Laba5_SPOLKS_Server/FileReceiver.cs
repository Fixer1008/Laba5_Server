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

        private FileDetails _fileDetails;

        public FileReceiver()
        {
            _udpFileReceiver = new UdpFileClient(11000);
            _udpFileSender = new UdpFileClient();
        }

        public int ReceiveFrom()
        {
            ReceiveFileDetails();

            Console.WriteLine(_fileDetails.FileName);
            Console.WriteLine(_fileDetails.FileLength);

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

                _fileDetails = (FileDetails)serializer.Deserialize(memoryStream);
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
                if (_fileDetails.FileLength > 0)
                {
                    _fileStream = new FileStream(_fileDetails.FileName, FileMode.Append, FileAccess.Write);

                    IPAddress clientIpAddress = IPAddress.Parse(ClientIp);
                    IPEndPoint clientEndPoint = new IPEndPoint(clientIpAddress, 5000);
                    _udpFileSender.Connect(clientEndPoint);

                    for (_fileStream.Position = 0; _fileStream.Position < _fileDetails.FileLength; )
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
