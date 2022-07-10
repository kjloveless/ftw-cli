using System.Security.Cryptography;
using System.Text;

namespace ftw_msgr.Crypto;

public class Crypt
{
    ECDiffieHellman localKey;
    ECDiffieHellmanPublicKey localPublicKey;
    public string localPublicKey_b64;
    public ECDiffieHellmanPublicKey? remotePublicKey { get; set; }

    public Crypt()
    {
        localKey = ECDiffieHellman.Create();
        localPublicKey = localKey.PublicKey; 
        localPublicKey_b64 = System.Convert.ToBase64String(localPublicKey.ToByteArray());
    }

    public string DecryptMessage(string ciphertext)
    {
        byte[] data = System.Convert.FromBase64String(ciphertext);
        byte[] rawData;

        using (var aes = AesCng.Create())
        {
            var ivLength = aes.BlockSize >> 3;
            byte[] ivData = new byte[ivLength];
            Array.Copy(data, ivData, ivLength);


            var sumKey = localKey.DeriveKeyMaterial(remotePublicKey);
            aes.Key = sumKey;
            aes.IV = ivData;
            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            using (MemoryStream ms = new MemoryStream())
            {
                var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
                cs.Write(data, ivLength, data.Length - ivLength);
                cs.Close();
                rawData = ms.ToArray();
                return Encoding.UTF8.GetString(rawData);
            }
        }
    }

    public string? EncryptMessage(string message)
    {
        byte[] rawData = Encoding.UTF8.GetBytes(message);
        var sumKey = localKey.DeriveKeyMaterial(remotePublicKey);

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
                    
                    return System.Convert.ToBase64String(data);
                }
                aes.Clear();
            }
        }
    }

    public void InitRemotePublicKey(byte[] remotePubKey)
    {
        // last place where there is a windows only class
        remotePublicKey = ECDiffieHellmanCngPublicKey.FromByteArray(remotePubKey, new CngKeyBlobFormat("ECCPUBLICBLOB")); 
    }
}
