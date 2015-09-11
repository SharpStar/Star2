using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StarLib.Security
{
    /// <summary>
	/// Security helper methods for Starbound
	/// </summary>
	public static class StarSecurity
    {
        public static byte[] EmptySalt => new byte[24];

        public static byte[] GenerateHash(string account, string password, byte[] salt)
        {
            byte[] passAcct = Encoding.UTF8.GetBytes(password + account);

            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(passAcct.Concat(salt).ToArray());

            sha256.Dispose();
            
            return hash;
        }

        public static byte[] GenerateSalt(int length = 24)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] tokenData = new byte[length];
                rng.GetBytes(tokenData);
                
                return tokenData;
            }
        }
    }
}
