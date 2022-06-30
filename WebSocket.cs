using System;
using System.Net;
using System.Net.Sockets;

public class MsgrServer
{
    TcpClient? socket;
    public MsgrServer()
    {
        Console.WriteLine("client or server?");
        String? Line = Console.ReadLine();
        switch (Line)
        {
            case "client": SetupClient("localhost"); break;
            case "server": SetupServer(); break;
        }
    }

    private void SetupServer()
    {
        var listener = new TcpListener(IPAddress.Any, 50001);
        listener.Start();
        socket = listener.AcceptTcpClient();
        Task.Run(() => HandleRequest()); 
        Console.WriteLine("Connected to client from {0}...", socket.Client.RemoteEndPoint.ToString());
    }

    private void SetupClient(String ip)
    {
        try
        {
            socket = new TcpClient(ip, 50001);
            Task.Run(() => HandleRequest());
            Console.WriteLine("Connected to server...");
        }
        catch (SocketException e)
        {
            Console.WriteLine(e.ToString());
        }

    }

    private void HandleRequest()
    {
        var netStream = socket.GetStream();
        var reader = new BinaryReader(netStream);
        var writer = new BinaryWriter(netStream);

        while (socket.Connected)
        {
            var cmd = reader.ReadString();
            switch (cmd)
            {
                case "chat":
                    break;
                case "exit":
                    socket.Close();
                    break;
            }
        }
    }
}
