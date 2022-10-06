using ftw_msgr.WebSocket;
using ftw_msgr.Connection;

Base_Connection? msgrServer = null;

string? text;
do
{
  Console.Write("client or server? ");
  text = Console.ReadLine();
  if (string.IsNullOrWhiteSpace(text)) { text = ""; }
} while (text != "client" && text != "server");

switch (text)
{
  case "server":
    msgrServer = new Server();
    break;
  case "client":
    msgrServer = new Client();
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