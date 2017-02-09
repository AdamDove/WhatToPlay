using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WhatToPlay.ViewModel
{
    /// <summary>
    /// Source http://codereview.stackexchange.com/questions/15346/using-system-security-cryptography-protecteddata-do-you-see-any-issue-with-encr
    /// Written by: Ankush : http://codereview.stackexchange.com/users/12783/ankush
    /// </summary>
    /// <param name="password"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    class SecurePassword
    {
        static public string Encrypt(string password, string salt)
        {
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
            byte[] saltBytes = Encoding.Unicode.GetBytes(salt);

            byte[] cipherBytes = ProtectedData.Protect(passwordBytes, saltBytes, DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(cipherBytes);
        }

        static public string Decrypt(string cipher, string salt)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipher);
            byte[] saltBytes = Encoding.Unicode.GetBytes(salt);

            byte[] passwordBytes = ProtectedData.Unprotect(cipherBytes, saltBytes, DataProtectionScope.CurrentUser);

            return Encoding.Unicode.GetString(passwordBytes);
        }

        public static string ConvertToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        //[TestMethod()]
        //public void EncryptDecryptTest()
        //{
        //    string password = "Gussme!";
        //    string salt = new Random().Next().ToString();

        //    string cipher = Authenticator.Encrypt(password, salt);
        //    Assert.IsFalse(cipher.Contains(password), "Unable to encrypt");
        //    Assert.IsFalse(cipher.Contains(salt), "Unable to encrypt");

        //    string decipher = Authenticator.Decrypt(cipher, salt);
        //    Assert.AreEqual(password, decipher);
        //}
    }
}
