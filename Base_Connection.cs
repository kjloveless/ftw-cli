using System.Net.Sockets;
using System.Security.Cryptography;
using ftw_msgr.Crypto;


namespace ftw_msgr.Connection;
public class Base_Connection
{
  protected TcpClient? socket;
  protected NetworkStream? netStream;
  protected BinaryReader? reader;
  protected BinaryWriter? writer;
  protected List<string> messages;
  protected bool handleStarted;
  protected Crypt myCrypt;
  public List<string> MsgHistory => messages;

  protected void InitBaseConnection()
  {
    // Console.Clear();
    myCrypt = new Crypt();
    messages = new List<string>();
  }
  protected void SendKey()
  {
    if (myCrypt.localPublicKey_Q.X is not null && myCrypt.localPublicKey_Q.Y is not null)
    {
      SendMsg<string>("ECC_PUB_KEY", false);
      SendMsg<int>(myCrypt.localPublicKey_Q.X.Length, false);
      SendMsg<byte[]>(myCrypt.localPublicKey_Q.X, false);
      SendMsg<byte[]>(myCrypt.localPublicKey_Q.Y, false);
    }
    else
    {
      Console.WriteLine("Local public key not available.");
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
            if (myChars is null)
            {
              myBytes = Array.Empty<byte>();
            }
            else
            {
              myBytes = Convert.FromBase64CharArray(myChars, 0, myChars.Length);
            }
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
    ECPoint myECPoint;
    while (socket is not null && socket.Connected && reader is not null)
    {
      var cmd = reader.ReadString();
      //Console.Clear();
      if (cmd is not null)
      {
        switch (cmd)
        {
          case "exit":
            socket.Close();
            break;
        }
        if (cmd.Equals("ECC_PUB_KEY"))
        {
          int byteCount = reader.ReadInt32();
          byte[]? myX = reader.ReadBytes(byteCount);
          byte[]? myY = reader.ReadBytes(byteCount);
          myECPoint.X = myX;
          myECPoint.Y = myY;
          myCrypt.InitRemotePublicKey(myECPoint);
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
    }
  }
}
