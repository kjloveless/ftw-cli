MsgrServer msgrServer;
if (args is not null && args.Count() > 0 && args[0] is not null) 
{ 
    msgrServer = new MsgrServer(args[0]);
}
else {
    msgrServer = new MsgrServer();
}
string? text;

while (true) 
{
    Console.Write("Send: ");
    text = Console.ReadLine();
    msgrServer.MsgHistory.Add($"Send: {text}");
    if (text == null) break;
    msgrServer.SendMsg(text);
};