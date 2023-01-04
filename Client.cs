using System.Net.Sockets;
using ftw_msgr.Connection;

namespace ftw_msgr.WebSocket;

public class Client : Base_Connection
{
  public Client(string arg = "")
  {
    SetupClient(arg);

  }

  private void SetupClient(string ip = "")
  {
    string? line;
    try
    {
      Console.WriteLine("Enter an IP address to connect to...");
      if (ip is not null) 
      { 
        line = ip;
      }
      else 
      {
        line = Console.ReadLine();
      }
      ip = string.IsNullOrWhiteSpace(line) ? "localhost" : line;
      if (ip != "localhost")
      {
        try
        {
          socket = new TcpClient(ip, 1702);
        }
        catch (SocketException e)
        {
          Console.WriteLine(e.Message);
          socket = new TcpClient(ip, 50001);
        }
      }
      else
      {
        socket = new TcpClient(ip, 50001);
      }
      InitComs();

      Console.WriteLine("Connected to server...");
    }
    catch (SocketException e)
    {
      Console.WriteLine(e.ToString());
    }

  }
}
