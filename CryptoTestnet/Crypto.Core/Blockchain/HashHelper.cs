using System.Security.Cryptography;
using System.Text;

namespace Crypto.Core.Blockchain;
public static class HashHelper
{
    public static string CalculateHash(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}