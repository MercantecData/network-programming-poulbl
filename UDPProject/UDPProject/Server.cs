using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPProject
{
    class Server
    {

        public Server()
        {
            
            //byte[] bytes = Encoding.UTF8.GetBytes("hej");
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1243);
            UdpClient client = new UdpClient(endpoint);
            Receiver(client);
        }

        public void SendMessage(IPEndPoint endpoint)
        {

        }

        public async void Receiver(UdpClient client)
        {
            while (true)
            {
                Console.WriteLine("Ready");
                UdpReceiveResult result = await client.ReceiveAsync();
                byte[] buffer = result.Buffer;
                string text = Encoding.UTF8.GetString(buffer);

                Console.WriteLine("Received: " + text);
            }
        }


        static void Main(string[] args)
        {
            Server server = new Server();
            Console.ReadLine();
        }
    }
}
