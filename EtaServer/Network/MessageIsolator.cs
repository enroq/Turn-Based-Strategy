using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtaServer
{
    internal class MessageIsolator
    {
        static string m_EncryptedMessageTerminator = ",";

        int m_TerminatorLength = m_EncryptedMessageTerminator.Length;

        string[] m_Terminators 
            = new string[] { m_EncryptedMessageTerminator };

        ClientState m_ClientState;
        AesModule m_Aes;

        ClientMessageEventArgs 
            m_IsolationArgs = new ClientMessageEventArgs();

        internal static string EncryptedMessageTerminator { get { return m_EncryptedMessageTerminator; } }

        public MessageIsolator(ClientState state, AesModule aes)
        {
            m_ClientState = state;
            m_Aes = aes;
        }

        internal void ProcessStream()
        {
            RelayMessages(IsolateMessagesToEnum(m_ClientState.MessageQueue.ToString()));

            int lastTerminatorIndex = m_ClientState.MessageQueue.ToString().LastIndexOf(m_EncryptedMessageTerminator);
            m_ClientState.MessageQueue.Remove
                (0, lastTerminatorIndex + m_TerminatorLength);

            if(ServerCore.DebugMode)
                Console.WriteLine("({0}) Characters Left In Message Queue", m_ClientState.MessageQueue.Length);
        }

        [ObsoleteAttribute("This method is obsolete. Use IsolateMessagesToEnum Instead.", false)]
        internal string[] IsolateMessages(string data)
        {
            int terminatorCount = data.Split(m_Terminators, StringSplitOptions.None).Length - 1;

            string[] messages = new string[terminatorCount];

            for (int i = 0; i < terminatorCount; i++)
            {
                int terminatorIndex = data.IndexOf(m_EncryptedMessageTerminator);

                messages[i] = data.Substring(0, terminatorIndex);

                data = data.Remove(0, terminatorIndex + m_TerminatorLength);
            }

            if (messages.Length > 0)
                return messages;
            else
                return null;
        }

        internal IEnumerable<string> IsolateMessagesToEnum(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) { yield break; }

            int currentIndex = 0;
            int terminatorIndex = -1;

            while ((terminatorIndex = data.IndexOf(m_EncryptedMessageTerminator, currentIndex)) > 0)
            {
                int messageLength = terminatorIndex - currentIndex;

                yield return data.Substring(currentIndex, messageLength);

                currentIndex = terminatorIndex + m_TerminatorLength;
            }
        }


        [ObsoleteAttribute("This method is obsolete. Use RelayMessages(IEnumerable<string>) Instead.", false)]
        internal void RelayMessages(string[] messages)
        {
            if (messages == null)
                return;

            for (int i = 0; i < messages.Length; i++)
            {
                Console.WriteLine(messages[i]);
            }
        }

        internal void RelayMessages(IEnumerable<string> messages)
        {
            foreach (string message in messages)
            {
                m_IsolationArgs.UpdateMessage(m_ClientState.ClientId, m_Aes.DecryptStringToString(message));
                NetworkEventDispatcher.InvokeClientMessageEvent(m_IsolationArgs);

                if (ServerCore.DebugMode)
                    Console.WriteLine("Relay Event Invoked On Thread: " + Thread.CurrentThread.ManagedThreadId);
            }
        }
    }
}
