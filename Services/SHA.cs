using System.Security.Cryptography;
using System.Text;

namespace MatchingSystem.Services
{
    public class SHA
    {
        // 用于计算 SHA1 散列
        public static string ComputeSha1Hash(string input)
        {
            using (var sha1 = SHA1.Create())
            {
                var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashBytes);
            }
        }

        // 用于计算 SHA256 散列
        public static string ComputeSha256Hash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
