using ftw_msgr.WebSocket;
using ftw_msgr.Connection;

Base_Connection? msgrServer = null;

string? arg;
do  
{
    Console.Write("client or server? ");            
    arg = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(arg)) { arg = ""; }
} while (arg != "client" && arg != "server");

switch (arg)
{
    case "server": 
      msgrServer = new Server(); 
      break;
    case "client": 
      msgrServer = new Client(); 
      break;
}


string? text;

while (true && msgrServer is not null) 
{
    Console.Write("Send: ");
    text = Console.ReadLine();
    msgrServer.MsgHistory.Add($"Send: {text}");
    if (text == null) break;
    msgrServer.SendMsg(text);
};