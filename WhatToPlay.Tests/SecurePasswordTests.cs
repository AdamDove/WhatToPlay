using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WhatToPlay.ViewModel;

namespace WhatToPlay.Tests
{
    [TestFixture]
    public class SecurePasswordTests
    {
        [Test]
        public void EncryptAndDecryptWorks()
        {
            SecureString sourcePassword = new SecureString();
            sourcePassword.AppendChar('a');
            sourcePassword.AppendChar('b');
            sourcePassword.AppendChar('c');
            sourcePassword.AppendChar('d');
            string encryptedString = SecurePassword.Encrypt(sourcePassword, "salt1");

            //make sure the encrypted string is not un-encrypted.
            Assert.AreNotEqual("abcd", encryptedString);

            SecureString resultPassword = SecurePassword.Decrypt(encryptedString, "salt1");
            String sourcePasswordText = SecurePassword.ToPlainText(sourcePassword);
            String resultPasswordText = SecurePassword.ToPlainText(resultPassword);
            Assert.AreEqual(sourcePasswordText, resultPasswordText);

        }

        [Test]
        public void SaltMatters()
        {
            SecureString sourcePassword = new SecureString();
            sourcePassword.AppendChar('a');
            sourcePassword.AppendChar('b');
            sourcePassword.AppendChar('c');
            sourcePassword.AppendChar('d');

            string encryptedString = SecurePassword.Encrypt(sourcePassword, "salt1");
            bool exceptionThrown = false;
            try
            {
                SecureString resultPassword = SecurePassword.Decrypt(encryptedString, "salt2");
            }
            catch (CryptographicException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "Using the wrong salt should throw exception.");
        }
    }
}
