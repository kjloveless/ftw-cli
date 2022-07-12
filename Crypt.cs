using System.Security.Cryptography;
using System.Text;

namespace ftw_msgr.Crypto;

public class Crypt
{
    ECDiffieHellman localKey;
    ECDiffieHellmanPublicKey localPublicKey;
    public ECPoint localPublicKey_Q;
    public ECDiffieHellman? RemoteKey { get; set; }
    public ECDiffieHellmanPublicKey? remotePublicKey { get; set; }

    public Crypt()
    {
        localKey = ECDiffieHellman.Create();
        localPublicKey = localKey.PublicKey;
        localPublicKey_Q = localPublicKey.ExportParameters().Q;
        RemoteKey = ECDiffieHellman.Create();
    }

    public string DecryptMessage(string ciphertext)
    {
        byte[] data = System.Convert.FromBase64String(ciphertext);
        byte[] rawData;

        using var aes = AesCng.Create();
        
        var ivLength = aes.BlockSize >> 3;
        byte[] ivData = new byte[ivLength];
        Array.Copy(data, ivData, ivLength);


        var sumKey = localKey.DeriveKeyMaterial(remotePublicKey);
        aes.Key = sumKey;
        aes.IV = ivData;
        using ICryptoTransform decryptor = aes.CreateDecryptor();
        using MemoryStream ms = new MemoryStream();
        
        var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
        cs.Write(data, ivLength, data.Length - ivLength);
        cs.Close();
        rawData = ms.ToArray();
        return Encoding.UTF8.GetString(rawData);
        
        
    }

    public T? EncryptMessage<T>(T rawMessage)
    {
        if (typeof(T) == typeof(string))
        {
            string? message = rawMessage as string;
            if (message is null) { message = ""; }
            byte[] rawData = Encoding.UTF8.GetBytes(message);
            var sumKey = localKey.DeriveKeyMaterial(remotePublicKey);

            using var aes = AesCng.Create();
            
            aes.Key = sumKey;
            aes.GenerateIV();
            using ICryptoTransform encryptor = aes.CreateEncryptor();
            
            using MemoryStream ms = new MemoryStream();
            
            var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            ms.Write(aes.IV, 0, aes.IV.Length);
            cs.Write(rawData, 0, rawData.Length);
            cs.Close();
            var data = ms.ToArray();
            return (T?)Convert.ChangeType(Convert.ToBase64String(data), typeof(T));
                    
                
            
        }
        else if (typeof(T) == typeof(byte[]))
        {
            char[]? chars = rawMessage as char[];
            if (chars is null) { chars = Array.Empty<char>(); }
            byte[] rawData = Encoding.UTF8.GetBytes(chars);
            var sumKey = localKey.DeriveKeyMaterial(remotePublicKey);

            using var aes = AesCng.Create();
            
            aes.Key = sumKey;
            aes.GenerateIV();
            using ICryptoTransform encryptor = aes.CreateEncryptor();
            
            using MemoryStream ms = new MemoryStream();
            
            CryptoStream? cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            ms.Write(aes.IV, 0, aes.IV.Length);
            cs.Write(rawData, 0, rawData.Length);
            cs.Close();
            byte[]? myBytes = ms.ToArray();
            char[]? myChars = new char[myBytes.Length];
            Convert.ToBase64CharArray(myBytes, 0, myBytes.Length, myChars, 0);
            return (T?)Convert.ChangeType(myChars, typeof(T));
                    
                
            
        } else
        {
            return default;
        }
    }

    public void InitRemotePublicKey(ECPoint myQ)
    {
        var ecdh = ECDiffieHellman.Create();
        ECParameters myECParams = new() { Q = myQ };
        ECCurve curve = localKey.ExportParameters(false).Curve;
        myECParams.Curve = curve;
        ecdh.ImportParameters(myECParams);
        remotePublicKey = ecdh.PublicKey;
    }
}
