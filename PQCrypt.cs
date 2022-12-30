using System.Text;

using System.Security.Cryptography;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber;
using Org.BouncyCastle.Security;

namespace ftw_msgr.Crypto;

public class PQCrypt
{
  private static SecureRandom random = new SecureRandom();
  private static KyberKemGenerator KyberEncCipher = new KyberKemGenerator(random);
  private static KyberParameters kyberParameters = KyberParameters.kyber512_aes;
   private AsymmetricCipherKeyPair localACKP;
   public KyberPublicKeyParameters localPubParams;
   public byte[]? secret;


  public PQCrypt()
  {
    KyberKeyPairGenerator kpGen = new KyberKeyPairGenerator();
    KyberKeyGenerationParameters genParam = new KyberKeyGenerationParameters(random, kyberParameters);
    kpGen.Init(genParam);
    localACKP = kpGen.GenerateKeyPair();

    localPubParams = (KyberPublicKeyParameters)localACKP.Public;
    KyberPrivateKeyParameters privParams = (KyberPrivateKeyParameters)localACKP.Private;
  }

  public string DecryptMessage(string ciphertext)
  {
    // return $"Unable to decrypt: {ciphertext} .\n No remote public key found.";
    byte[] data = System.Convert.FromBase64String(ciphertext);
    byte[] rawData;

    using Aes aes = AesCng.Create();

    int ivLength = aes.BlockSize >> 3;
    byte[] ivData = new byte[ivLength];
    Array.Copy(data, ivData, ivLength);

    if (secret is not null)
    {
      aes.Key = secret;
      aes.IV = ivData;
      using ICryptoTransform decryptor = aes.CreateDecryptor();
      using MemoryStream ms = new MemoryStream();

      CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
      cs.Write(data, ivLength, data.Length - ivLength);
      cs.Close();
      rawData = ms.ToArray();
      return Encoding.UTF8.GetString(rawData);
    }
    else
    {
      return $"Unable to decrypt: {ciphertext} .\n No remote public key found.";
    }
  }

  public T? EncryptMessage<T>(T rawMessage)
  {
    if (secret is not null)
    {
        using Aes aes = AesCng.Create();

        aes.Key = secret;
        aes.GenerateIV();
        using ICryptoTransform encryptor = aes.CreateEncryptor();

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
            byte[] data = ms.ToArray();
            return (T?)Convert.ChangeType(Convert.ToBase64String(data), typeof(T));
        }
        else if (typeof(T) == typeof(byte[]))
        {
            char[]? chars = rawMessage as char[];
            if (chars is null) { chars = Array.Empty<char>(); }
            byte[] rawData = Encoding.UTF8.GetBytes(chars);

            ms.Write(aes.IV, 0, aes.IV.Length);
            cs.Write(rawData, 0, rawData.Length);
            cs.Close();
            byte[]? myBytes = ms.ToArray();
            char[]? myChars = new char[myBytes.Length];
            Convert.ToBase64CharArray(myBytes, 0, myBytes.Length, myChars, 0);
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

  public void InitRemotePublicKey(byte[] encodedRemotePubParams)
  {
    KyberPublicKeyParameters remotePubParams = new KyberPublicKeyParameters(kyberParameters, encodedRemotePubParams);
    ISecretWithEncapsulation secWenc = KyberEncCipher.GenerateEncapsulated(remotePubParams);
    byte[] generated_cipher_text = secWenc.GetEncapsulation();
    secret = secWenc.GetSecret();
  }
}
