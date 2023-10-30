using System.Text;

using System.Security.Cryptography;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber;
using Org.BouncyCastle.Security;

namespace ftw_msgr.Crypto;

/// <summary>
/// This unit is the current encryption unit and uses Kyber KEM with AES.
/// </summary>

public class PQCrypt
{
    private static SecureRandom random = new SecureRandom();
    private static KyberKemGenerator KyberEncCipher = new KyberKemGenerator(random);
    private static KyberParameters kyberParameters = KyberParameters.kyber1024_aes;
    private AsymmetricCipherKeyPair localACKP;
    public KyberPublicKeyParameters pubParams;
    private KyberPrivateKeyParameters privParams;
    public byte[]? secret;
    public byte[]? generated_cipher_text;
    public string? encoded_cipher_text;


    public PQCrypt()
    {
        KyberKeyPairGenerator kpGen = new KyberKeyPairGenerator();
        KyberKeyGenerationParameters genParam = new KyberKeyGenerationParameters(random, kyberParameters);
        kpGen.Init(genParam);
        localACKP = kpGen.GenerateKeyPair();

        pubParams = (KyberPublicKeyParameters)localACKP.Public;
        privParams = (KyberPrivateKeyParameters)localACKP.Private;
    }

    public string DecryptMessage(string ciphertext)
    {
        using Aes aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 256;

        byte[] cipher = Convert.FromBase64String(ciphertext);
        byte[] rawData;

        int ivLength = aes.BlockSize >> 3;
        byte[] ivData = new byte[ivLength];
        Array.Copy(cipher, ivData, ivLength);

        if (secret is not null)
        {
            aes.Key = secret;
            aes.IV = ivData;

            ICryptoTransform decryptor = aes.CreateDecryptor();

            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);

            cs.Write(cipher, ivLength, cipher.Length - ivLength);
            cs.FlushFinalBlock();
            rawData = ms.ToArray();
            return $"{Encoding.UTF8.GetString(rawData)}";
        }
        else
        {
            return $"Unable to decrypt: {ciphertext} .\n No secret found.";
        }
    }

    public T? EncryptMessage<T>(T rawMessage)
    {
        if (secret is not null)
        {
            using Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 256;

            aes.Key = secret;
            aes.GenerateIV();

            ICryptoTransform encryptor = aes.CreateEncryptor();

            byte[] encryptedData;

            using MemoryStream ms = new MemoryStream();

            CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);

            if (typeof(T) == typeof(string))
            {
                string? message = rawMessage as string;
                if (message is null) { message = ""; }
                byte[] rawData = Encoding.UTF8.GetBytes(message);

                ms.Write(aes.IV, 0, aes.IV.Length);
                cs.Write(rawData, 0, rawData.Length);
                cs.Close();
                encryptedData = ms.ToArray();

                return (T?)Convert.ChangeType(Convert.ToBase64String(encryptedData), typeof(T));
            }
            else if (typeof(T) == typeof(byte[]))
            {
                char[]? chars = rawMessage as char[];
                if (chars is null) { chars = Array.Empty<char>(); }
                byte[] rawData = Encoding.UTF8.GetBytes(chars);

                ms.Write(aes.IV, 0, aes.IV.Length);
                cs.Write(rawData, 0, rawData.Length);
                cs.Close();
                encryptedData = ms.ToArray();
                char[]? myChars = new char[encryptedData.Length];
                Convert.ToBase64CharArray(encryptedData, 0, encryptedData.Length, myChars, 0);

                return (T?)Convert.ChangeType(myChars, typeof(T));
            }
            else
            {
                return default;
            }
        }
        else
        {
            return default;
        }
    }

    public void InitRemoteSecret()
    {
        if (generated_cipher_text is not null)
        {
            KyberKemExtractor KyberDecCipher = new KyberKemExtractor(privParams);
            secret = KyberDecCipher.ExtractSecret(generated_cipher_text);
        }
    }

    public void InitRemotePublicKey(byte[] encodedRemotePubParams)
    {
        pubParams = new KyberPublicKeyParameters(kyberParameters, encodedRemotePubParams);
        ISecretWithEncapsulation secWenc = KyberEncCipher.GenerateEncapsulated(pubParams);
        generated_cipher_text = secWenc.GetEncapsulation();
        encoded_cipher_text = Convert.ToBase64String(generated_cipher_text);
        secret = secWenc.GetSecret();
    }
}
