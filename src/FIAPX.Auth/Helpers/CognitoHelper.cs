using System.Security.Cryptography;
using System.Text;

namespace FIAPX.Auth.Helpers
{
    public class CognitoHelper
    {
        public static string GenerateSecretHash(string username, string clientId, string clientSecret)
        {
            var message = username + clientId;
            var keyBytes = Encoding.UTF8.GetBytes(clientSecret);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
