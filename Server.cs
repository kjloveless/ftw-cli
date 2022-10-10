using System.Net;
using System.Net.Sockets;
using Open.Nat;
using ftw_msgr.Connection;

namespace ftw_msgr.WebSocket;

public class Server : Base_Connection
{
  public Server(string arg = "")
  {
    SetupServer();
  }

  private async void SetupServer()
  {
    NatDiscoverer discoverer = new NatDiscoverer();
    try
    {
      // using SSDP protocol, it discovers NAT device.
      NatDevice device = await discoverer.DiscoverDeviceAsync();

      // display the NAT's IP address
      Console.WriteLine("The external IP Address is: {0} ", await device.GetExternalIPAsync());

      // create a new mapping in the router [external_ip:1702 -> host_machine:1602]
      await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 50001, 1702, "For testing"));
    }
    catch (NatDeviceNotFoundException e)
    {
      Console.WriteLine(e.Message);
    }

    TcpListener listener = new TcpListener(IPAddress.Any, 50001);
    listener.Start();
    socket = listener.AcceptTcpClient();
    InitComs();
    Console.WriteLine($"Connected to client from {socket.Client.RemoteEndPoint?.ToString()}...");

    SendKey();
  }
}
