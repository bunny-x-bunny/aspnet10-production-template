using System.Security.Cryptography;
using System.Text;

namespace Common.Helpers {
  public static class SecureRandomStringGenerator {
    private const string Chars = "abcdefghijklmnopqrstuvwxyz0123456789";

    public static string Generate(int length) {
      var result = new char[length];
      byte[] randomBytes = new byte[length];
      RandomNumberGenerator.Fill(randomBytes);

      for (int i = 0; i < length; i++) {
        int randomIndex = randomBytes[i] % Chars.Length;
        result[i] = Chars[randomIndex];
      }

      return new string(result);
    }
  }
}