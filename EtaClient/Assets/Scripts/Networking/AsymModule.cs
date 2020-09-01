using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public class AsymModule
{
    static RSACryptoServiceProvider
        m_Rsa = new RSACryptoServiceProvider();

    private static string m_PublicKey;

    internal static string PublicKey { get { return m_PublicKey; } }

    internal static string EncryptStringToString(string data)
    {
        return Convert.ToBase64String
            (m_Rsa.Encrypt(Encoding.UTF8.GetBytes(data), false));
    }

    internal static bool IsPublicKey(string key)
    {
        try
        {
            m_PublicKey = key;
            m_Rsa.FromXmlString(m_PublicKey);

            EventSink.InvokeStandardLogEvent
                (new LogEventArgs("Key Received: " + key));

            return true;
        }

        catch (Exception e)
        {
            EventSink.InvokeStandardLogEvent(new LogEventArgs(e.ToString()));
            return false;
        }
    }
}
