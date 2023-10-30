using ftw_msgr.WebSocket;
using ftw_msgr.Connection;

Base_Connection? msgrServer = null;

string? text;
do
{
  Console.Write("client or server? ");
  if (args is not null && args.Count() > 0 && args[0] is not null) 
  { 
      text = args[0];
  }
  else 
  {
    text = Console.ReadLine();
  }
  if (string.IsNullOrWhiteSpace(text)) { text = ""; }
} while (text != "client" && text != "server");

switch (text)
{
  case "server":
    msgrServer = new Server();
    break;
  case "client":
    // if (args is not null && args.Count() > 0 && args[1] is not null) 
    // { 
    //   msgrServer = new Client(args[1]);
    // }
    // else
    // {
      msgrServer = new Client();
    // }
    break;
}

text = "";
while (msgrServer is not null)
{
  Console.Write("Send: ");
  text = Console.ReadLine();
  msgrServer.MsgHistory.Add($"Send: {text}");
  if (text == null) break;
  msgrServer.SendMsg(text);
};