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
        private const int LocalPort = 11000;
        private const string SyncMessage = "SYNC";

        private readonly UdpFileClient _udpFileReceiver;

        private int connectionFlag = 0;

        private FileStream _fileStream;
        private IPEndPoint _remoteIpEndPoint = null;

        public FileDetails FileDetails { get; set; }

        public FileReceiver()
        {
            _udpFileReceiver = new UdpFileClient(LocalPort);
        }

        void InitializeUdpClients()
        {
            _udpFileReceiver.Client.ReceiveTimeout = _udpFileReceiver.Client.SendTimeout = 10000;
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
                    _udpFileReceiver.Connect(_remoteIpEndPoint);

                    if (_udpFileReceiver.ActiveRemoteHost)
                    {
                        _fileStream = new FileStream(FileDetails.FileName, FileMode.Append, FileAccess.Write);

                        for (_fileStream.Position = 0; _fileStream.Position < FileDetails.FileLength; )
                        {
                            try
                            {
                                var fileDataArray = _udpFileReceiver.Receive(ref _remoteIpEndPoint);

                                filePointer += fileDataArray.Length;
                                Console.WriteLine(filePointer);

                                _fileStream.Write(fileDataArray, 0, fileDataArray.Length);

                                var sendBytesAmount = _udpFileReceiver.Send(Encoding.UTF8.GetBytes(SyncMessage), SyncMessage.Length);
                            }
                            catch (SocketException e)
                            {
                                if (e.SocketErrorCode == SocketError.TimedOut && connectionFlag < 3)
                                {
                                    _udpFileReceiver.Connect(_remoteIpEndPoint);

                                    if (_udpFileReceiver.ActiveRemoteHost)
                                    {
                                        connectionFlag = 0;
                                    }
                                    else
                                    {
                                        connectionFlag++;
                                    }

                                    continue;
                                }
                                else
                                {
                                    _udpFileReceiver.Close();
                                    _fileStream.Close();
                                    _fileStream.Dispose();
                                    return -1;
                                }
                            }
                        }
                    }
                    else
                    {
                        _udpFileReceiver.Close();
                        return -1;
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
