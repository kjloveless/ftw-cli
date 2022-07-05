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
    string? localPublicKey_b64;
    // CngKey bobKey;
    byte[]? remotePublicKey;
    bool handleStarted;

    public MsgrServer(string arg = "")
    {   
        // Console.Clear();
        messages = new List<String>();     
        localKey = CngKey.Create(CngAlgorithm.ECDiffieHellmanP256);
        localPublicKey = localKey.Export(CngKeyBlobFormat.EccPublicBlob); 
        localPublicKey_b64 = System.Convert.ToBase64String(localPublicKey);
        Console.WriteLine($"local_pub_ley => {Encoding.Default.GetString(localPublicKey)}");
        Console.WriteLine("client or server?");
        String? Line;
        if (arg != "") Line = arg;
        else
        {
            Line = Console.ReadLine();
        }
        
        switch (Line)
        {
            case "client": SetupClient("localhost"); break;
            case "server": SetupServer(); break;
        }
    }

    private string DecryptMessage(string ciphertext)
    {
        byte[] data = Encoding.Default.GetBytes(ciphertext);
        byte[] rawData;

        using (var aes = AesCng.Create())
        {
            var ivLength = aes.BlockSize >> 3;
            byte[] ivData = new byte[ivLength];
            Array.Copy(data, ivData, ivLength);

            using (ECDiffieHellmanCng cng = new ECDiffieHellmanCng(localKey))
            {
                using (CngKey remoteKey = CngKey.Import(remotePublicKey, CngKeyBlobFormat.EccPublicBlob))
                {
                    var sumKey = cng.DeriveKeyMaterial(remoteKey);
                    aes.Key = sumKey;
                    aes.IV = ivData;
                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
                        cs.Write(data, ivLength, data.Length - ivLength);
                        // cs.Close();
                        rawData = ms.ToArray();
                        return Encoding.Default.GetString(rawData);
                    }
                }
            }
        }
    }

    private string? EncryptMessage(string message)
    {
        byte[] rawData = Encoding.Default.GetBytes(message);
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
                                return Encoding.Default.GetString(data);
                            }
                        }
                    }
                }
            }
            return null;
        }
    }

    public void SendMsg(string? msg, bool encrypt = true)
    {
        while (!handleStarted) continue;
        if ((writer is not null) && (msg is not null)) 
        {
            if (encrypt)
            {
                msg = EncryptMessage(msg);
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
        SendMsg($"ECC_PUB_KEY_{localPublicKey_b64}", false);
    }

    private void SetupClient(String ip)
    {
        try
        {
            socket = new TcpClient(ip, 50001);
            InitComs();
            SendMsg($"ECC_PUB_KEY_{localPublicKey_b64}", false);
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
            // Console.Clear();
            if (cmd is not null && cmd.StartsWith("ECC_PUB_KEY_"))
            {
                // Console.WriteLine(cmd);
                remotePublicKey = System.Convert.FromBase64String(cmd.Split("ECC_PUB_KEY_")[1]);
                // Console.WriteLine(Encoding.Default.GetString(remotePublicKey));
            } else
            {
                cmd = DecryptMessage(cmd);
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
