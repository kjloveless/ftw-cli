using System.Security.Cryptography;
using System.Text;

CngKey aliceKey;
byte[] alicePublicKey;
CngKey bobKey;
byte[] bobPublicKey;

void CreateKey()
{
    aliceKey = CngKey.Create(CngAlgorithm.ECDiffieHellmanP256);
    alicePublicKey = aliceKey.Export(CngKeyBlobFormat.EccPublicBlob);
    bobKey = CngKey.Create(CngAlgorithm.ECDiffieHellmanP256);
    bobPublicKey = bobKey.Export(CngKeyBlobFormat.EccPublicBlob);
}

void ReceiveData(byte[] data)
{
    Console.WriteLine("Bob recieves it and starts decrypting...");
    byte[] rawData;

    using (var aes = AesCng.Create())
    {
        var ivLength = aes.BlockSize >> 3;
        byte[] ivData = new byte[ivLength];
        Array.Copy(data, ivData, ivLength);

        using (ECDiffieHellmanCng cng = new ECDiffieHellmanCng(bobKey))
        {
            using (CngKey aliceKey = CngKey.Import(alicePublicKey, CngKeyBlobFormat.EccPublicBlob))
            {
                var sumKey = cng.DeriveKeyMaterial(aliceKey);
                aes.Key = sumKey;
                aes.IV = ivData;
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream ms = new MemoryStream())
                {
                    var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
                    cs.Write(data, ivLength, data.Length - ivLength);
                    cs.Close();
                    rawData = ms.ToArray();
                    Console.Write("The decryption is successful and the information is: ");
                    Console.WriteLine(Encoding.UTF8.GetString(rawData)+"\n");
                }
            }
        }
    }
}

void SendMessage(string message)
{
    byte[] rawData = Encoding.UTF8.GetBytes(message);
    using (ECDiffieHellmanCng cng = new ECDiffieHellmanCng(aliceKey))
    {
        using (CngKey bobKey = CngKey.Import(bobPublicKey, CngKeyBlobFormat.EccPublicBlob))
        {
            var sumKey = cng.DeriveKeyMaterial(bobKey);

            using (var aes = AesCng.Create())
            {
                aes.Key = sumKey;
                aes.GenerateIV();
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                        ms.Write(aes.IV, 0, aes.IV.Length);
                        cs.Write(rawData, 0, rawData.Length);
                        cs.Close();
                        var data = ms.ToArray();
                        ReceiveData(data);
                    }
                    aes.Clear();
                }
            }
        }
    }
}

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
    if (text == null) break;
    msgrServer.SendMsg(text);
};

//CreateKey();
//SendMessage("The weather today is good!");
//SendMessage("La la la, f-t-w");
//Pause();
