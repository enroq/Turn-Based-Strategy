using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

internal class MessageIsolator
{
    static string m_MessageTerminator = ClientManager.EncryptedMessageTerminator;

    int m_TerminatorLength = m_MessageTerminator.Length;

    string[] m_Terminators 
        = new string[] { m_MessageTerminator };

    ClientState m_ClientState;

    public MessageIsolator(ClientState state)
    {
        m_ClientState = state;
    }

    internal void ProcessStream()
    {
        RelayMessages(IsolateMessagesToEnum(m_ClientState.MessageQueue.ToString()));

        int lastTerminatorIndex = m_ClientState.MessageQueue.ToString().LastIndexOf(m_MessageTerminator);
        m_ClientState.MessageQueue.Remove
            (0, lastTerminatorIndex + m_TerminatorLength);
    }

    [ObsoleteAttribute("This method is obsolete. Use IsolateMessagesToEnum Instead.", false)]
    internal string[] IsolateMessages(string data)
    {
        int terminatorCount = data.Split(m_Terminators, StringSplitOptions.None).Length - 1;

        string[] messages = new string[terminatorCount];

        for (int i = 0; i < terminatorCount; i++)
        {
            int terminatorIndex = data.IndexOf(m_MessageTerminator);

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
        if (string.IsNullOrEmpty(data)) { yield break; }

        int currentIndex = 0;
        int terminatorIndex = -1;

        while ((terminatorIndex = data.IndexOf(m_MessageTerminator, currentIndex)) > 0)
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
            string dMsg = ClientManager.DecryptToString(message);

            EventSink.InvokeServerMessageEvent(new ServerMessageEventArgs(dMsg));
        }
    }
}
