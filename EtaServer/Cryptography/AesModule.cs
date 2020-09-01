using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EtaServer
{
    internal class AesModule
    {
        private AesCryptoServiceProvider m_Aes;

        private static string m_FileName = "AesKeyIV.txt";

        private static string m_CryptDocCache;

        internal void InitializeProvider()
        {
            m_Aes = new AesCryptoServiceProvider();

            m_Aes.BlockSize = 128;
            m_Aes.KeySize = 256;
            m_Aes.GenerateIV();
            m_Aes.GenerateKey();
            m_Aes.Mode = CipherMode.CBC;
            m_Aes.Padding = PaddingMode.PKCS7;
        }

        internal void InitializeProviderFromFile()
        {
            if (File.Exists(m_FileName))
            {
                string s = File.ReadAllText(m_FileName);
                string[] segments = s.Split(':');

                InitializeProviderWithKey(segments[0], segments[1]);
                Console.WriteLine("Aes Key And Vector Loaded From File..");
            }

            else
                Console.WriteLine("[Error]: Aes Key-Vector File Can Not Be Found.");
        }

        internal void InitializeProviderWithKey(string key, string iv)
        {
            try
            {
                m_Aes = new AesCryptoServiceProvider();

                m_Aes.BlockSize = 128;
                m_Aes.KeySize = 256;
                m_Aes.IV = Convert.FromBase64String(iv);
                m_Aes.Key = Convert.FromBase64String(key);
                m_Aes.Mode = CipherMode.CBC;
                m_Aes.Padding = PaddingMode.PKCS7;
            }

            catch(Exception e) { Console.WriteLine(e.ToString()); }
        }

        internal void GenerateProviderAndOutput()
        {
            InitializeProvider();
            OutputProviderKeyAndVector();
        }

        internal void OutputProviderKeyAndVector()
        {
            try
            {
                string s = 
                    Convert.ToBase64String(m_Aes.Key)
                    + ":" +
                    Convert.ToBase64String(m_Aes.IV);

                File.WriteAllText(m_FileName, s);
            }

            catch(Exception e) { Console.WriteLine(e.ToString()); }
        }

        internal string EncryptStringToString(string input)
        {
            try
            {
                ICryptoTransform transform = m_Aes.CreateEncryptor();

                return Convert.ToBase64String
                    (transform.TransformFinalBlock(Encoding.ASCII.GetBytes(input), 0, input.Length));
            }

            catch (Exception e) { Console.WriteLine(e.ToString()); return null; }
        }

        internal string DecryptStringToString(string input)
        {
            try
            {
                ICryptoTransform transform = m_Aes.CreateDecryptor();

                byte[] encoded = Convert.FromBase64String(input);

                return Encoding.ASCII.GetString
                    (transform.TransformFinalBlock(encoded, 0, encoded.Length));
            }

            catch(Exception e) { Console.WriteLine(e.ToString()); return null; }
        }

        internal void EncryptFile(string filename)
        {
            if (File.Exists(filename))
            {
                m_CryptDocCache = File.ReadAllText(filename);
                File.WriteAllText
                    (filename, EncryptStringToString(m_CryptDocCache));

                Console.WriteLine("File Successfully Encrypted.");
            }

            else
                Console.WriteLine("File Specified Does Not Exist.");
        }

        internal void DecryptFile(string filename)
        {
            if (File.Exists(filename))
            {
                m_CryptDocCache = File.ReadAllText(filename);
                File.WriteAllText
                    (filename, DecryptStringToString(m_CryptDocCache));

                Console.WriteLine("File Sucessfully Decrypted.");
            }

            else
                Console.WriteLine("File Specified Does Not Exist.");
        }

        internal void ParseDecryptCommand(string command)
        {
            string[] data = command.Split(' ');

            try
            {
                DecryptFile(data[1]);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        internal void ParseEncryptCommand(string command)
        {
            string[] data = command.Split(' ');

            try
            {
                EncryptFile(data[1]);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
