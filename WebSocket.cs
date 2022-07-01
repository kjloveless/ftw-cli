using System;
using System.Net;
using System.Net.Sockets;

public class MsgrServer
{
    TcpClient? socket;
    NetworkStream? netStream;
    BinaryReader? reader;
    BinaryWriter? writer;
    List<String> messages; 

    public MsgrServer()
    {   
        messages = new List<String>();        
        Console.WriteLine("client or server?");
        String? Line = Console.ReadLine();
        switch (Line)
        {
            case "client": SetupClient("localhost"); break;
            case "server": SetupServer(); break;
        }
    }

    public void SendMsg(String msg)
    {
        if (writer is not null) 
        {
            writer.Write(msg);
        } else 
        {
            InitComs();
            if (writer is not null)
            {
                writer.Write(msg);
            } else
            {
                Console.WriteLine("writer not initialized.");
            }
        }
    }

    public List<String> MsgHistory => messages;

    private void SetupServer()
    {
        var listener = new TcpListener(IPAddress.Any, 50001);
        listener.Start();
        socket = listener.AcceptTcpClient();
        InitComs();
        Task.Run(() => HandleRequest()); 
        Console.WriteLine("Connected to client from {0}...", socket.Client.RemoteEndPoint.ToString());
    }

    private void SetupClient(String ip)
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
        } else
        {
            Console.WriteLine("Socket not initialized.");
        }
    }

    private void HandleRequest()
    {
        while (socket.Connected)
        {         
            var cmd = reader.ReadString();
            messages.Add(String.Format("Client: {0}", cmd));
            Console.Clear();
            foreach (var msg in messages)
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
