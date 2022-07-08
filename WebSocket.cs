using System;
using System.Net;
using System.Net.Sockets;
using ftw_msgr.Crypto;

namespace ftw_msgr.WebSocket;

public class MsgrServer
{
    TcpClient? socket;
    NetworkStream? netStream;
    BinaryReader? reader;
    BinaryWriter? writer;
    List<String> messages; 
    bool handleStarted;
    Crypt myCrypt;

    public MsgrServer(string arg = "")
    {   
        // Console.Clear();
        myCrypt = new Crypt();
        messages = new List<String>();     
        Console.WriteLine("client or server?");
        String? Line;
        if (arg != "") Line = arg;
        else
        {
            Line = Console.ReadLine();
        }
        
        switch (Line)
        {
            case "client": SetupClient(); break;
            case "server": SetupServer(); break;
        }
    }

    public void SendMsg(string? msg, bool encrypt = true)
    {
        while (!handleStarted) continue;
        if ((writer is not null) && (msg is not null)) 
        {
            if (encrypt)
            {
                msg = myCrypt.EncryptMessage(msg);
            }
            writer.Write(msg);
        } else 
        {
            Console.WriteLine("writer not initialized.");
        }
    }

    public List<String> MsgHistory => messages;

    private void SetupServer()
    {
        var listener = new TcpListener(IPAddress.Any, 50001);
        listener.Start();
        socket = listener.AcceptTcpClient();
        InitComs();
        Console.WriteLine($"Connected to client from {socket.Client.RemoteEndPoint?.ToString()}...");
        SendMsg($"ECC_PUB_KEY_{myCrypt.localPublicKey_b64}", false);
    }

    private void SetupClient(String ip = "")
    {
        try
        {
            Console.WriteLine("Enter an IP address to connect to...");
            ip = Console.ReadLine();
            if (ip != "localhost") 
            {
                socket = new TcpClient();
                socket.Connect(IPAddress.Parse(ip), 50001);    
            }
            else
            {
                socket = new TcpClient(ip, 50001);
            }
            InitComs();
            SendMsg($"ECC_PUB_KEY_{myCrypt.localPublicKey_b64}", false);
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

            Task.Run(() => HandleRequest()); 
        } else
        {
            Console.WriteLine("Socket not initialized.");
        }
    }

    private void HandleRequest()
    {
        handleStarted = true;
        while (socket is not null && socket.Connected)
        {         
            var cmd = reader?.ReadString();
            Console.Clear();
            if (cmd is not null && cmd.StartsWith("ECC_PUB_KEY_"))
            {
               myCrypt.remotePublicKey = System.Convert.FromBase64String(cmd.Split("ECC_PUB_KEY_")[1]);
            } else
            {
                cmd = myCrypt.DecryptMessage(cmd);
                messages.Add(String.Format("Client: {0}", cmd));
            }
            
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
