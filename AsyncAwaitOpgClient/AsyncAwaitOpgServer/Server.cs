using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncAwaitOpgClient
{
    class Server
    {
        List<TcpClient> clients = new List<TcpClient>();
        List<NetworkStream> streams = new List<NetworkStream>();

        public Server()
        {
            int port = 13356;
            IPAddress ip = IPAddress.Any;
            IPEndPoint localEndpoint = new IPEndPoint(ip, port);
            TcpListener listener = new TcpListener(localEndpoint);
            listener.Start();

            connectClient(listener);
            ReceiveMessage(streams);
        }
        public void connectClient(TcpListener listener)
        {
            Task<TcpClient> getClient = listener.AcceptTcpClientAsync();
            TcpClient client = getClient.Result;
            clients.Add(client);
            OpenStream(client);
        }

        public void OpenStream(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            streams.Add(stream);
        }

        public async void ReceiveMessage(List<NetworkStream> streams)
        {
            foreach(NetworkStream stream in streams)
            {
                byte[] buffer = new byte[256];
                int numberOfBytesRead = await stream.ReadAsync(buffer, 0, 256);
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, numberOfBytesRead);

                Console.WriteLine(receivedMessage);
              
            }
        }

    }
}
