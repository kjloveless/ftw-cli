using System.Security.Cryptography;
using System.Text;

// void ReceiveData(byte[] data)
// {
//     Console.WriteLine("Bob recieves it and starts decrypting...");
//     byte[] rawData;

//     using (var aes = AesCng.Create())
//     {
//         var ivLength = aes.BlockSize >> 3;
//         byte[] ivData = new byte[ivLength];
//         Array.Copy(data, ivData, ivLength);

//         using (ECDiffieHellmanCng cng = new ECDiffieHellmanCng(bobKey))
//         {
//             using (CngKey aliceKey = CngKey.Import(alicePublicKey, CngKeyBlobFormat.EccPublicBlob))
//             {
//                 var sumKey = cng.DeriveKeyMaterial(aliceKey);
//                 aes.Key = sumKey;
//                 aes.IV = ivData;
//                 using (ICryptoTransform decryptor = aes.CreateDecryptor())
//                 using (MemoryStream ms = new MemoryStream())
//                 {
//                     var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
//                     cs.Write(data, ivLength, data.Length - ivLength);
//                     cs.Close();
//                     rawData = ms.ToArray();
//                     Console.Write("The decryption is successful and the information is: ");
//                     Console.WriteLine(Encoding.UTF8.GetString(rawData)+"\n");
//                 }
//             }
//         }
//     }
// }

// void SendMessage(string message)
// {
//     
// }

void Pause()
{
    Console.WriteLine("Press any button to continue...");
    Console.Read();
}

MsgrServer msgrServer = new MsgrServer();
string? text;

while (true) 
{
    Console.Write("Send: ");
    text = Console.ReadLine();
    msgrServer.MsgHistory.Add($"Send: {text}");
    Console.WriteLine(Encoding.UTF8.GetString(msgrServer.remotePublicKey));
    if (text == null) break;
    msgrServer.SendMsg(text);
};

//CreateKey();
//SendMessage("The weather today is good!");
//SendMessage("La la la, f-t-w");
//Pause();
