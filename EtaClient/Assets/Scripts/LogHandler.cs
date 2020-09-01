using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.IO;
using System;

public class LogHandler : MonoBehaviour
{
	void Start ()
    {
        EventSink.StandardLogEvent += EventSink_StandardLogEvent;
	}

    private void EventSink_StandardLogEvent(LogEventArgs args)
    {
        ClientManager.Post(() =>
        {
            Debug.Log(args.Message);
        });

        //string filename = string.Format("output-{0}.txt", DateTime.Now.ToLongTimeString());

        //try
        //{
        //    if (File.Exists(filename))
        //        File.AppendAllText(filename, string.Format("[{0}]: {1}{2}{2}",
        //            DateTime.Now.ToShortTimeString(), args.Message, Environment.NewLine));
        //    else
        //        File.WriteAllText(filename, string.Format("[{0}]: {1}{2}{2}",
        //            DateTime.Now.ToShortTimeString(), args.Message, Environment.NewLine));
        //}

        //catch (Exception e)
        //{

        //}
    }
}
