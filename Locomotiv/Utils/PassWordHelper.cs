using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Utils
{
   
    public static class PassWordHelper
    {
        public static (string Hash, string Salt) HashPassword(string password)
        {
            // Generate a 16-byte salt
            byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
            string salt = Convert.ToBase64String(saltBytes);

            // Combine password + salt
            string saltedPassword = password + salt;
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(saltedPassword));
            string hash = Convert.ToBase64String(hashBytes);

            return (hash, salt);
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
        {
            string saltedPassword = enteredPassword + storedSalt;
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(saltedPassword));
            string computedHash = Convert.ToBase64String(hashBytes);

            return computedHash == storedHash;
        }
    }

    
}
