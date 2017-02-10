using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WhatToPlay.ViewModel
{
    public class SecurePassword
    {
        String salt = "WhatToPlayPasswordSalt";
        SecureString localPassword;

        public SecurePassword()
        {

        }

        public SecurePassword(SecureString securePassword, bool rememberPassword)
        {
            localPassword = securePassword;
            Properties.Settings.Default.RememberMe = rememberPassword;
            if (rememberPassword)
                Save();
            else
                Delete();
        }

        public void Load()
        {
            localPassword = Decrypt(Properties.Settings.Default.EncryptedPassword, salt);
        }

        public void Save()
        {
            Properties.Settings.Default.RememberMe = true;
            Properties.Settings.Default.EncryptedPassword = Encrypt(localPassword, salt);
            Properties.Settings.Default.Save();

        }
        public void Delete()
        {
            Properties.Settings.Default.RememberMe = false;
            Properties.Settings.Default.EncryptedPassword = "";
            Properties.Settings.Default.Save();
        }

        public static string Encrypt(SecureString secureString, string salt)
        {
            IntPtr unmanagedPasswordString = IntPtr.Zero;
            try
            {
                unmanagedPasswordString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                byte[] saltBytes = Encoding.Unicode.GetBytes(salt);

                //this next line is all done in one big stack so no variable ever holds the unencrypted value.
                byte[] encryptedBytes = ProtectedData.Protect(
                    Encoding.Unicode.GetBytes(                          //convert the managed string to a byte array
                        Marshal.PtrToStringUni(unmanagedPasswordString) //convert the unmanaged string to a managed string
                    ),
                    saltBytes, 
                    DataProtectionScope.CurrentUser);

                return Convert.ToBase64String(encryptedBytes);
            }
            finally
            {
                //clear text passwords are now all cleared out of memory again.
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedPasswordString);
            }
        }
        public static SecureString Decrypt(string encryptedString, string salt)
        {
            byte[] managedInsecurePasswordBytes;
            string managedInsecurePasswordString;
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedString);
                byte[] saltBytes = Encoding.Unicode.GetBytes(salt);
                managedInsecurePasswordBytes = ProtectedData.Unprotect(encryptedBytes, saltBytes, DataProtectionScope.CurrentUser);
                managedInsecurePasswordString = System.Text.Encoding.Unicode.GetString(managedInsecurePasswordBytes);
                SecureString secureString = new SecureString();
                foreach (char c in managedInsecurePasswordString)
                {
                    secureString.AppendChar(c);
                }
                secureString.MakeReadOnly();
                return secureString;
            }
            finally
            {
                managedInsecurePasswordBytes = null;
                managedInsecurePasswordString = null;
            }
        }
        public string ToPlainText()
        {
            return ToPlainText(localPassword);
        }
        public static string ToPlainText(SecureString password)
        {
            IntPtr unmanagedPasswordString = IntPtr.Zero;
            try
            {
                unmanagedPasswordString = Marshal.SecureStringToGlobalAllocUnicode(password);
                return Marshal.PtrToStringUni(unmanagedPasswordString);
            }
            finally
            {
                //clear text passwords are now all cleared out of memory again.
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedPasswordString);
            }
        }
    }
}
