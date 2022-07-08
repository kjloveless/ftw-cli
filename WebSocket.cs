using System;
using System.Net;
using System.Net.Sockets;

namespace ftw_msgr.WebSocket
{
    public class MsgrServer
    {
        TcpClient? socket;
        NetworkStream? netStream;
        BinaryReader? reader;
        private BinaryWriter? writer;

        public MsgrServer()
        {
            MsgHistory = new List<string>();
            Console.WriteLine("client or server?");
            string? Line = Console.ReadLine();
            switch (Line)
            {
                case "client": SetupClient("localhost"); break;
                case "server": SetupServer(); break;
            }
        }

        public void SendMsg(string msg)
        {
            if (writer is not null)
            {
                writer.Write(msg);
            }
            else
            {
                InitComs();
                if (writer is not null)
                {
                    writer.Write(msg);
                }
                else
                {
                    Console.WriteLine("writer not initialized.");
                }
            }
        }

        public List<string> MsgHistory { get; }

        private void SetupServer()
        {
            var listener = new TcpListener(IPAddress.Any, 50001);
            listener.Start();
            socket = listener.AcceptTcpClient();
            InitComs();
            Task.Run(() => HandleRequest());
            Console.WriteLine($"Connected to client from {socket.Client.RemoteEndPoint}...");
        }

        private void SetupClient(string ip)
        {
            try
            {
                socket = new TcpClient(ip, 50001);
                InitComs();
                Task.Run(() => HandleRequest());
                Console.WriteLine("Connected to server...");
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        private void InitComs()
        {
            if (socket is not null)
            {
                netStream = socket.GetStream();
                reader = new BinaryReader(netStream);
                writer = new BinaryWriter(netStream);
            }
            else
            {
                Console.WriteLine("Socket not initialized.");
            }
        }

        private void HandleRequest()
        {
            if (socket is null) return;

            while (socket.Connected)
            {
                if (reader is null) break;

                var cmd = reader.ReadString();
                MsgHistory.Add($"Client: {cmd}");
                Console.Clear();
                foreach (var msg in MsgHistory)
                {
                    Console.WriteLine(msg);
                }

                Console.Write("Send: ");

                switch (cmd)
                {
                    case "exit":
                        socket.Close();
                        break;
                }
            }
        }
    }
}