using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Geldmaat
{
    internal class Tools
    {

        static public bool VerifySHA256Hash(string dataToVerify, string expectedHash)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToVerify));
                string computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                return computedHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
            }
        }



        public string CreateSHA256Hash(string dataToHash)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
                string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return hashString;
            }
        }
    }
}
