using System.Security.Cryptography;
using System.Text;

// void ReceiveData(byte[] data)
// {
//     
// }

// void SendMessage(string message)
// {
//     
// }

// void Pause()
// {
//     Console.WriteLine("Press any button to continue...");
//     Console.Read();
// }

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

//CreateKey();
//SendMessage("The weather today is good!");
//SendMessage("La la la, f-t-w");
//Pause();
