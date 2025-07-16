using System.Security.Cryptography;

using TheChatbot.Infra;

namespace TheChatbot.Entities;

public class Encryption {
  public byte[] Key;
  public byte[] IV;
  public Encryption(string key, string iv) {
    Key = Convert.FromBase64String(key);
    IV = Convert.FromBase64String(iv);
    if (Key == null || Key.Length != 32) {
      throw new DeveloperException(
        "[Encryption]",
        action: "Check the encryption Key since it's empty when it should be a byte[]."
      );
    }
    if (IV == null || IV.Length != 16) {
      throw new DeveloperException(
        "[Encryption]",
        action: "Check the encryption IV since it's empty when it should be a byte[]."
      );
    }
  }
  public string Encrypt(string plainText) {
    if (plainText.Length <= 0) throw new ValidationException("The ecryption text should be valid");
    byte[] encrypted;
    using (Aes aesAlg = Aes.Create()) {
      aesAlg.Key = Key;
      aesAlg.IV = IV;
      var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
      using var msEncrypt = new MemoryStream();
      using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
      using (var swEncrypt = new StreamWriter(csEncrypt)) {
        swEncrypt.Write(plainText);
      }
      encrypted = msEncrypt.ToArray();
    }
    return Convert.ToBase64String(encrypted);
  }

  public string Decrypt(string cipherText) {
    var cipherBytes = Convert.FromBase64String(cipherText);
    if (cipherBytes.Length <= 0) throw new ValidationException("The ecryption text should be valid");
    string? plaintext = null;
    using (Aes aesAlg = Aes.Create()) {
      aesAlg.Key = Key;
      aesAlg.IV = IV;
      var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
      using var msDecrypt = new MemoryStream(cipherBytes);
      using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
      using var srDecrypt = new StreamReader(csDecrypt);
      plaintext = srDecrypt.ReadToEnd();
    }
    return plaintext;
  }
}
