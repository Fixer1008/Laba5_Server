using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;

namespace Laba5_SPOLKS_Server
{
    public class FileReceiver
    {
        private readonly UdpFileClient _udpFileClient;
        private readonly FileStream _fileStream;

        private IPAddress _localIpAddress;
        private IPEndPoint _localIpEndPoint;
        private IPEndPoint _remoteIpEndPoint = null;

        public FileReceiver()
        {
            _udpFileClient = new UdpFileClient(11000);
        }

        public int ReceiveFrom()
        {
            var fileDetails = ReceiveFileDetails();
            Console.WriteLine(fileDetails.FileName);
            Console.WriteLine(fileDetails.FileLength);
            return 0;
        }

        private FileDetails ReceiveFileDetails()
        {
            var receivedFileInfo = _udpFileClient.Receive(ref _remoteIpEndPoint);
            _udpFileClient.Close();

            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(receivedFileInfo, 0, receivedFileInfo.Length);

            XmlSerializer serializer = new XmlSerializer(typeof(FileDetails));

            FileDetails fileDetails = (FileDetails)serializer.Deserialize(memoryStream);

            return fileDetails;
        }
    }
}
