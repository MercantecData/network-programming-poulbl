using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPClient
{
    class Client
    {
        public Client()
        {
            UdpClient client = new UdpClient();

            string text = "Hello UDP D:";
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1243);
            Send(client, bytes, endpoint);
        }

        public async void Send(UdpClient client, byte[] bytes, IPEndPoint endpoint)
        {
            await client.SendAsync(bytes, bytes.Length, endpoint);
        }

        static void Main(string[] args)
        {
            Client client = new Client();
        }
    }
}
