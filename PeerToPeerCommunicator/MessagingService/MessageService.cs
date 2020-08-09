using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using SimpleTCP;
using Message = MessagingService.Message;

namespace MessagingService
{
    public sealed class MessageService
    {
        private readonly SimpleTcpServer myServer;

        public MessageService()
        {
            myServer = new SimpleTcpServer { StringEncoder = Encoding.UTF8 };
            myServer.DataReceived += MyServerOnDataReceived;
        }

        public event EventHandler<Message> MessageReceived;    

        public void StartListening(int localPort)
        {
            StartListening("127.0.0.1", localPort);
        }

        public void StartListening(string localIpAddress, int localPort)
        {
            try
            {
                IPAddress localAddress = IPAddress.Parse(localIpAddress);
                myServer.Start(localAddress, localPort);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }

        public static Message CreateNewMessage(string senderName, string messageText, string filePath)
        {
            Message message = new Message
            {
                SenderName = senderName,
                MessageText = messageText,
                SendingTime = DateTime.Now.ToString("dd MM yyyy HH:mm:ss")
            };

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                string fileInBase64 = Convert.ToBase64String(fileBytes);
                message.FileDataInBase64 = fileInBase64;
                message.FileName = Path.GetFileName(filePath);
            }
            return message;
        }

        private void MyServerOnDataReceived(object sender, SimpleTCP.Message e)
        {
            Message receivedMessage = Message.Deserialize(e.MessageString);
            MessageReceived?.Invoke(this, receivedMessage);
            StoreReceivedFile(receivedMessage);
        }

        private void StoreReceivedFile(Message message)
        {
            if (string.IsNullOrEmpty(message.FileDataInBase64) || string.IsNullOrEmpty(message.FileName))
            {
               return; 
            }

            byte[] fileBytes = Convert.FromBase64String(message.FileDataInBase64);
            File.WriteAllBytes($"{GetSystemDownloadsPath()}\\{message.FileName}", fileBytes);
        }

        private string GetSystemDownloadsPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
        }


        public void StopListening()
        {
            if (myServer.IsStarted)
            {
                myServer?.Stop();
            }
        }

        public bool SendMessage(Message message, string remoteIpAddress, int remotePort)
        {
            try
            {
                string serializeMessage = message.Serialize();
                Byte[] data = Encoding.UTF8.GetBytes(serializeMessage);
                TcpClient client = new TcpClient(remoteIpAddress, remotePort);
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                stream.Close();
                client.Close();

                return true;
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
                return false;
            }
        }


        
    }
}