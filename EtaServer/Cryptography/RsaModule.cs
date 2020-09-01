using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EtaServer
{
    internal class RSAModule
    {
        static RSACryptoServiceProvider 
            m_Rsa = new RSACryptoServiceProvider();

        static RSAParameters m_RsaParams;

        static string m_PrivateRsa = string.Empty;
        static string m_PublicRsa = string.Empty;

        static readonly string m_PublicRsaFileName = "rsa_public.xml";
        static readonly string m_PrivateRsaFileName = "rsa_private.xml";

        static string m_CryptDocCache = string.Empty;

        internal static string GetPublicRsa
        {
            get { return m_PublicRsa; }
        }

        internal static bool LoadRsaKeyFromFile()
        {
            if(File.Exists(m_PublicRsaFileName))
                m_PublicRsa = File.ReadAllText(m_PublicRsaFileName);

            if(File.Exists(m_PrivateRsaFileName))
            {
                m_PrivateRsa = File.ReadAllText(m_PrivateRsaFileName);
                return 
                    true;
            }

            return 
                false;
        }

        internal static void GenerateNewRsaKeys()
        {
            m_Rsa = new RSACryptoServiceProvider();
            m_RsaParams = m_Rsa.ExportParameters(false);

            m_PrivateRsa = m_Rsa.ToXmlString(true);
            m_PublicRsa = m_Rsa.ToXmlString(false);
        }

        internal static void OutputCurrentRsaKeys()
        {
            if(!String.IsNullOrEmpty(m_PublicRsa) 
                && !String.IsNullOrEmpty(m_PrivateRsa))
            {
                File.WriteAllText
                    (m_PublicRsaFileName, m_PublicRsa);

                File.WriteAllText
                    (m_PrivateRsaFileName, m_PrivateRsa);
            }
        }

        internal static string EncryptStringToString(string data)
        {
            m_Rsa.FromXmlString(m_PublicRsa);
            return Convert.ToBase64String
                (m_Rsa.Encrypt(Encoding.UTF8.GetBytes(data), false));
        }

        internal static string DecryptStringToString(string data)
        {
            m_Rsa.FromXmlString(m_PrivateRsa);
            return Encoding.UTF8.GetString
                (m_Rsa.Decrypt(Convert.FromBase64String(data), false));
        }

        internal static byte[] EncryptStringToBytes(string data)
        {
            m_Rsa.FromXmlString(m_PublicRsa);
            return m_Rsa.Encrypt
                (Encoding.UTF8.GetBytes(data), false);
        }

        internal static byte[] DecryptBytesToBytes(byte[] data)
        {
            m_Rsa.FromXmlString(m_PrivateRsa);
            return 
                m_Rsa.Decrypt(data, false);
        }

        internal static void EncryptFile(string filename)
        {
            if(File.Exists(filename))
            {
                m_CryptDocCache = File.ReadAllText(filename);
                File.WriteAllText
                    (filename, EncryptStringToString(m_CryptDocCache));

                Console.WriteLine("File Successfully Encrypted.");
            }

            else
                Console.WriteLine("File Specified Does Not Exist.");
        }

        internal static void DecryptFile(string filename)
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

        internal static void ParseDecryptCommand(string command)
        {
            string[] data = command.Split(' ');

            try
            {
                DecryptFile(data[1]);
            }

            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        internal static void ParseEncryptCommand(string command)
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

        internal static bool QueryRsa()
        {
            if (LoadRsaKeyFromFile())
            {
                Console.WriteLine("Asymmetric Keys Successfully Loaded..");
                return true;
            }

            else
            {
                Console.WriteLine("Warning: Unable To Load Asymmetric Keys..");
                return false;
            }
        }
    }
}
