using System.Security.Cryptography;
using System.Text;

namespace LinuxHub.Common.Helpers
{
    public static class CryptoHelper
    {
        public static string GenerateSha512Hash(string input)
        {
            using var sha = SHA512.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
