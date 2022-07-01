using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

public class MsgrServer
{
    TcpClient? socket;
    NetworkStream? netStream;
    BinaryReader? reader;
    BinaryWriter? writer;
    List<String> messages; 
    CngKey localKey;
    byte[] localPublicKey;
    // CngKey bobKey;
    byte[]? remotePublicKey;
    bool handleStarted;

    public MsgrServer()
    {   
        Console.Clear();
        messages = new List<String>();     
        localKey = CngKey.Create(CngAlgorithm.ECDiffieHellmanP256);
        localPublicKey = localKey.Export(CngKeyBlobFormat.EccPublicBlob);   
        Console.WriteLine("client or server?");
        String? Line = Console.ReadLine();
        switch (Line)
        {
            case "client": SetupClient("localhost"); break;
            case "server": SetupServer(); break;
        }
    }

    private string? EncryptMessage(string message)
    {
        byte[] rawData = Encoding.UTF8.GetBytes(message);
        using (ECDiffieHellmanCng cng = new ECDiffieHellmanCng(localKey))
        {
            if (remotePublicKey is not null)
            {
                using (CngKey remoteKey = CngKey.Import(remotePublicKey, CngKeyBlobFormat.EccPublicBlob))
                {
                    var sumKey = cng.DeriveKeyMaterial(remoteKey);

                    using (var aes = AesCng.Create())
                    {
                        aes.Key = sumKey;
                        aes.GenerateIV();
                        using (ICryptoTransform encryptor = aes.CreateEncryptor())
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                                ms.Write(aes.IV, 0, aes.IV.Length);
                                cs.Write(rawData, 0, rawData.Length);
                                cs.Close();
                                var data = ms.ToArray();
                                aes.Clear();
                                return ms.ToString();
                            }
                        }
                    }
                }
            }
            return null;
        }
    }

    public void SendMsg(String msg, bool encrypt = true)
    {
        while (!handleStarted) continue;
        if (writer is not null) 
        {
            if (encrypt)
            {
                // msg = EncryptMessage(msg);
            }
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
        Console.WriteLine($"Connected to client from {socket.Client.RemoteEndPoint?.ToString()}...");
        SendMsg($"ECC_PUB_KEY_{Encoding.UTF8.GetString(localPublicKey)}", false);
    }

    private void SetupClient(String ip)
    {
        try
        {
            socket = new TcpClient(ip, 50001);
            InitComs();
            SendMsg($"ECC_PUB_KEY_{Encoding.UTF8.GetString(localPublicKey)}", false);
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
                remotePublicKey = Encoding.UTF8.GetBytes(cmd.Split("ECC_PUB_KEY_")[1]);
            } else
            {
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
