using System.Security.Cryptography;
using System.Text;

namespace ftw_msgr.Crypto;

public class Crypt
{
    CngKey localKey;
    byte[] localPublicKey;
    public string localPublicKey_b64;
    public byte[]? remotePublicKey { get; set; }

    public Crypt()
    {
        localKey = CngKey.Create(CngAlgorithm.ECDiffieHellmanP256);
        localPublicKey = localKey.Export(CngKeyBlobFormat.EccPublicBlob); 
        localPublicKey_b64 = System.Convert.ToBase64String(localPublicKey);
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

            using (ECDiffieHellmanCng cng = new ECDiffieHellmanCng(localKey))
            {
                using (CngKey remoteKey = CngKey.Import(remotePublicKey, CngKeyBlobFormat.EccPublicBlob))
                {
                    var sumKey = cng.DeriveKeyMaterial(remoteKey);
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
        }
    }

    public string? EncryptMessage(string message)
    {
        byte[] rawData = Encoding.UTF8.GetBytes(message);
        using (ECDiffieHellmanCng cng = new ECDiffieHellmanCng(localKey))
        {
                using (CngKey remoteKey = CngKey.Import(remotePublicKey, CngKeyBlobFormat.EccPublicBlob))
                {
                    var sumKey = cng.DeriveKeyMaterial(remoteKey);

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
        }
    }
}
