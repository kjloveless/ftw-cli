using System.Net.Sockets;
using ftw_msgr.Crypto;


namespace ftw_msgr.Connection;
public class Base_Connection
{
  protected TcpClient? socket;
  protected NetworkStream? netStream;
  protected BinaryReader? reader;
  protected BinaryWriter? writer;
  protected List<string> messages = new List<string>();
  protected bool handleStarted;
  protected PQCrypt myCrypt = new PQCrypt();
  public List<string> MsgHistory => messages;

  // used by the server to move their public key to the client
  protected void SendKey()
  {
    if (myCrypt.pubParams is not null)
    {
      SendMsg<string>("KYBER_PUB_KEY", false);
      SendMsg<int>(myCrypt.pubParams.GetEncoded().Length, false);
      SendMsg<byte[]>(myCrypt.pubParams.GetEncoded(), false);
    }
    else
    {
      Console.WriteLine("Public key not available.");
    }
  }

  protected void SendCipher()
  {
    if (myCrypt.encoded_cipher_text is not null)
    {
      SendMsg<string>("KYBER_CIPHER", false);
      SendMsg<string>(myCrypt.encoded_cipher_text, false);
    }
    else
    {
      Console.WriteLine("Public key is not available to create cipher.");
    }
  }

  public void SendMsg<T>(T msg, bool encrypt = true)
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
      Type myType = typeof(T);
      switch (myType)
      {
        case Type when myType == typeof(byte[]):
          byte[]? myBytes = msg as byte[];
          if (myBytes is null) { myBytes = Array.Empty<byte>(); }
          char[]? myChars = new char[myBytes.Length * 2];
          if (encrypt)
          {
            Convert.ToBase64CharArray(myBytes, 0, myBytes.Length, myChars, 0);
            myChars = myCrypt.EncryptMessage<char[]>(myChars);
            myBytes = myChars is null ?
              Array.Empty<byte>()
              : Convert.FromBase64CharArray(myChars, 0, myChars.Length);

          }
          writer.Write(myBytes);
          break;
        case Type when myType == typeof(int):
          int myInt = 0;
          if (msg is not null)
          {
            myInt = Convert.ToInt32(msg);
          }
          writer.Write(myInt);
          break;
        case Type _ when myType == typeof(string):
          string? myStr = msg as string;
          if (myStr is null) { myStr = ""; }
          if (encrypt)
          {
            myStr = myCrypt.EncryptMessage<string>(myStr);
            if (myStr is null)
            {
              myStr = "";
            }
          }
          writer.Write(myStr);
          if (msg as string == "exit") { socket?.Close(); }
          break;
        default:
          break;
      }
    }
  }

  protected void InitComs()
  {
    if (socket is not null)
    {
      netStream = socket.GetStream();
      reader = new BinaryReader(netStream);
      writer = new BinaryWriter(netStream);

      Task.Run(() => HandleRequest());
    }
    else
    {
      Console.WriteLine("Socket not initialized.");
    }
  }

  protected void HandleRequest()
  {
    handleStarted = true;
    while (socket is not null && socket.Connected && reader is not null)
    {
      var cmd = reader.ReadString();
      if (cmd is not null)
      {
        int byteCount;
        switch (cmd)
        {
          case "exit":
            socket.Close();
            break;
          case "KYBER_PUB_KEY":
            byteCount = reader.ReadInt32();
            byte[]? myKey = reader.ReadBytes(byteCount);
            myCrypt.InitRemotePublicKey(myKey);
            SendCipher();
            break;
          case "KYBER_CIPHER":
            myCrypt.generated_cipher_text = Convert.FromBase64String(reader.ReadString());
            myCrypt.InitRemoteSecret();
            break;
          default:
            cmd = myCrypt.DecryptMessage(cmd);
            messages.Add(string.Format("Client: {0}", cmd));
            break;
        }
      }

      Console.Clear();
      foreach (string msg in messages)
      {
        Console.WriteLine(msg);
      }

      Console.Write("Send: ");
    }
  }
}
