using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using ftw_msgr.Crypto;

namespace ftw_msgr.WebSocket;

public class MsgrServer
{
    TcpClient? socket;
    NetworkStream? netStream;
    BinaryReader? reader;
    BinaryWriter? writer;
    List<string> messages; 
    bool handleStarted;
    Crypt myCrypt;

    public MsgrServer(string arg = "")
    {   
        // Console.Clear();
        myCrypt = new Crypt();
        messages = new List<string>();
        string? Line = arg;
        while (string.IsNullOrWhiteSpace(Line)) 
        {
            Console.Write("client or server? ");            
            Line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(Line)) { Line = ""; }
        }
        switch (Line)
        {
            case "client": SetupClient(); break;
            case "server": SetupServer(); break;
        }
    }

    public void SendMsg(string? msg, bool encrypt = true)
    {
        while (!handleStarted)
        {
            continue;
        }

        if (writer is null || msg is null)
        {
            Console.WriteLine("writer not initialized.");
        }
        else
        {
            if (encrypt)
            {
                msg = myCrypt.EncryptMessage(msg);
                if (msg is null)
                {
                    msg = "";
                }
            }
            writer.Write(msg);
        }
    }

    public List<string> MsgHistory => messages;

    private void SetupServer()
    {
        var listener = new TcpListener(IPAddress.Any, 50001);
        listener.Start();
        socket = listener.AcceptTcpClient();
        InitComs();
        Console.WriteLine($"Connected to client from {socket.Client.RemoteEndPoint?.ToString()}...");

        SendMsg("ECC_PUB_KEY", false);
        writer.Write(myCrypt.localPublicKey_X.Length);
        writer.Write(myCrypt.localPublicKey_X);
        writer.Write(myCrypt.localPublicKey_Y.Length);
        writer.Write(myCrypt.localPublicKey_Y);
    }

    private void SetupClient(string ip = "")
    {
        try
        {
            Console.WriteLine("Enter an IP address to connect to...");
            var line = Console.ReadLine();
            ip = (line is null) ? "" : line;
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

            SendMsg("ECC_PUB_KEY", false);
            writer.Write(myCrypt.localPublicKey_X.Length);
            writer.Write(myCrypt.localPublicKey_X);
            writer.Write(myCrypt.localPublicKey_Y.Length);
            writer.Write(myCrypt.localPublicKey_Y);
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
        ECParameters myECParams = new();
        while (socket is not null && socket.Connected)
        {         
            var cmd = reader?.ReadString();
            // Console.Clear();
            if (cmd is not null)
            {
                if (cmd.Equals("ECC_PUB_KEY"))
                {
                    int byteCount = reader.ReadInt32();
                    byte[]? myX = reader.ReadBytes(byteCount);
                    byteCount = reader.ReadInt32();
                    byte[]? myY = reader.ReadBytes(byteCount);
                    myECParams.Q.X = myX;
                    myECParams.Q.Y = myY;
                    myCrypt.InitRemotePublicKey(myECParams);
                }
                else if (cmd is not null)
                {
                    cmd = myCrypt.DecryptMessage(cmd);
                    messages.Add(string.Format("Client: {0}", cmd));
                }
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
