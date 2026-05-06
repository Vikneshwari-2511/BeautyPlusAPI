using System.Security.Cryptography;
using System.Text;

namespace BeautyPlusParlour.Helpers;

public static class HashHelper
{
    public static string ToSha256Base64(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    public static bool VerifySha256(string input, string storedHash) =>
        ToSha256Base64(input) == storedHash;
}