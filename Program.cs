using ftw_msgr.WebSocket;

namespace ftw_msgr
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MsgrServer msgrServer = new MsgrServer();
            string? text;

            while (true)
            {
                Console.Write("Send: ");
                text = Console.ReadLine();
                msgrServer.MsgHistory.Add($"Send: {text}");
                if (text == null) break;
                msgrServer.SendMsg(text);
            };
        }
    }
}