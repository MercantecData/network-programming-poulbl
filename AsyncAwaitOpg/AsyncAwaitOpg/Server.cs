using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace AsyncAwaitOpg
{
    public struct Client
    {
        public TcpClient client { get; }
        public NetworkStream stream { get; }

        public Client(TcpClient client, NetworkStream stream)
        {
            this.client = client;
            this.stream = stream;
        }

        public void DisconnectClient(bool bIsSocketConnected)
        {
            if(bIsSocketConnected)
            {
                client.GetStream().Close();
            }
            client.Close();
        }
    }

    class Server
    {
        List<Client> clients = new List<Client>();
        
        public Server()
        {
            Console.WriteLine("Starting server...");
            int port = 13356;
            IPAddress ip = IPAddress.Any;
            IPEndPoint localEndpoint = new IPEndPoint(ip, port);
            TcpListener listener = new TcpListener(localEndpoint);
            listener.Start();

            connectClient(listener);
            ReceiveMessage();

        }


        public Client getClientFromStream(NetworkStream stream)
        {
            Client clientToReturn = new Client();
            foreach(Client client in clients.ToArray())
            {
                if (client.stream == stream)
                {
                    clientToReturn = client;
                }
            }
            return clientToReturn;
        }
        public Client getClientFromClient(TcpClient clientToFind)
        {
            Client clientToReturn = new Client();
            foreach (Client client in clients.ToArray())
            {
                if (client.client == clientToFind)
                {
                    clientToReturn = client;
                }
            }
            return clientToReturn;
        }

        public List<NetworkStream> getStreams()
        {
            List<NetworkStream> streams = new List<NetworkStream>();
            foreach(Client client in clients.ToArray())
            {
                streams.Add(client.stream);
            }

            return streams;
        }
        

        public async void connectClient(TcpListener listener)
        {
            Console.WriteLine("Waiting for Client");
            TcpClient c = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Client connected");
            NetworkStream stream = c.GetStream();
            Client client = new Client(c, stream);
            clients.Add(client);
            connectClient(listener);
        }

        public async void ReceiveMessage()
        {

            while (true)
            {
                foreach (NetworkStream stream in getStreams().ToArray())
                {
                    if (stream.DataAvailable)
                    {

                        byte[] buffer = new byte[256];
                        int numberOfBytesRead = await stream.ReadAsync(buffer, 0, 256);
                        byte type = buffer[0];

                        string receivedMessage = Encoding.UTF8.GetString(buffer, 5, numberOfBytesRead);

                        if (type == 10)
                        {
                            List<NetworkStream> streamsList = getStreams();
                            BroadcastMessage(streamsList, receivedMessage, type);
                        }
                        else if (type == 3)
                        {
                            WriteMessage(stream, "Server: Pong", 3);
                        }
                        else if (type == 255)
                        {
                            DisconnectClient(stream, true);
                        }
                        else
                        {
                            Console.WriteLine("No valid type found");
                        }

                    }
                }
            }        
        }
        public async void WriteMessage(NetworkStream stream, string message, int type)
        {
            byte[] tlvlength = new byte[4];
            byte tlvType = Convert.ToByte(type);
            byte[] byteMessage = Encoding.UTF8.GetBytes(message);
            byte[] buffer = new byte[1 + tlvlength.Length + byteMessage.Length];
            buffer[0] = tlvType;

            tlvlength.CopyTo(buffer, 1);
            byteMessage.CopyTo(buffer, 5);

            try
            {
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch(System.IO.IOException e)
            {
                DisconnectClient(stream, false);
            }
        }

        public void BroadcastMessage(List<NetworkStream> streams, string message, int type)
        {
            foreach(NetworkStream stream in streams)
            {

                WriteMessage(stream, message, type);
            }
        }

        public void DisconnectClient(NetworkStream stream, bool bIsDisconnectClean)
        {
            Console.WriteLine("Disconnecting client...");
            Client client = getClientFromStream(stream);

            if(bIsDisconnectClean)
            {
                WriteMessage(stream, "Disconnect", 255);
            }

            client.DisconnectClient(bIsDisconnectClean);
            clients.Remove(client);
            Console.WriteLine("Client disconencted.");
        }

        public void DisconnectClient(TcpClient client)
        {
            throw new NotImplementedException();
        }

        public void DisconnectClient(Client client)
        {
            throw new NotImplementedException();
        }

        public static void Main()
        {
            Server server = new Server();
        }
    }
}

