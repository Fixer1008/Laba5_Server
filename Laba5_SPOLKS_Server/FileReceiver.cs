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
        private readonly UdpFileClient _udpFileReceiver;
        private readonly UdpFileClient _udpFileSender;

        private FileStream _fileStream;
        private IPAddress _localIpAddress;
        private IPEndPoint _localIpEndPoint;
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
            try
            {
                var fileDataArray = _udpFileReceiver.Receive(ref _remoteIpEndPoint);

                if (fileDataArray.Length != 0)
                {
                    _fileStream = new FileStream(_fileDetails.FileName, FileMode.Create, FileAccess.Write);
                    _fileStream.Write(fileDataArray, 0, fileDataArray.Length);

                    Process.Start(_fileDetails.FileName);
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
