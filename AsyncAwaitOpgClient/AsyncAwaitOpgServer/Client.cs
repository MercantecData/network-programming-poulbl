using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncAwaitOpgServer
{
    class Client
    {
        private string Name;
        public Client()
        {
            Name = namePrompt();
            int port = 13356;
            TcpClient client = new TcpClient();
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            
            Task<NetworkStream> getStream = AsyncConnect(client, ip, port);
            NetworkStream stream = getStream.Result;

            //client.Close();

            ReceiveMessage(stream);
            SendMessage(stream);
        }

        public string namePrompt()
        {
            Console.WriteLine("Write your name");
            return Console.ReadLine();
        }

        public async void SendMessage(NetworkStream stream)
        {
            while(true)
            {
                int type = 10; //10 = text message
                byte[] tlvlength = new byte[4];

                string text = Console.ReadLine();
                string nameText = (Name + ": " + text);
                byte[] buffer = Encoding.UTF8.GetBytes(Name + ": " + text);

                byte[] buffer2 = new byte[1 + tlvlength.Length + buffer.Length];

                if(text.ToLower() == "ping")
                {
                    type = 3;
                }

                else if(text.ToLower() == "disconnect")
                {
                    type = 255;
                }

                byte tlvtype = Convert.ToByte(type);
                buffer2[0] = tlvtype;
                tlvlength.CopyTo(buffer2, 1);
                buffer.CopyTo(buffer2, 5);

                await stream.WriteAsync(buffer2, 0, buffer2.Length);
            }
        }

        public async Task<NetworkStream> AsyncConnect(TcpClient client, IPAddress ip, int port)
        {
            await client.ConnectAsync(ip, port);

            NetworkStream stream = client.GetStream();

            return stream;
        }


        private async void ReceiveMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[256];
            while (true)
            {
                int numOfBytesRead = await stream.ReadAsync(buffer, 0, 256);
                string receivedMessage = Encoding.UTF8.GetString(buffer, 5, numOfBytesRead);
                byte type = buffer[0];
                if (type == 10)
                {
                    Console.WriteLine("message");
                    Console.WriteLine(receivedMessage);
                }
                else if (type == 255)
                {
                    Console.WriteLine("I should Disconnect");
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine(receivedMessage);
                }
            }
        }

        static void Main(string[] args)
        {
            Client client = new Client();
        }
    }
}
