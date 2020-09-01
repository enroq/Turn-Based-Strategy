using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtaServer
{
    class Diagnostics
    {
        internal static void OutputThreadCountData()
        {
            int maxWorkerThreads, maxCompletionPorts, availableWorkerThreads, availableCompletionPorts;
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPorts);
            ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableCompletionPorts);

            Console.WriteLine("=====================");
            Console.WriteLine("Number Of Threads In Use: [{0}]",
                Process.GetCurrentProcess().Threads.Count.ToString());

            Console.WriteLine("Worker Threads (available/max): [{1}/{0}]", maxWorkerThreads, availableWorkerThreads);
            Console.WriteLine("Completion Port Threads (available/max): [{1}/{0}]", maxCompletionPorts, availableCompletionPorts);
            Console.WriteLine("=====================");
        }

        internal static void OutputIndividualThreadData()
        {
            foreach (ProcessThread thread in Process.GetCurrentProcess().Threads)
            {
                string threadInfo = string.Format("{0} \n {1} \n {2} \n {3} \n {4}",
                    thread.Id.ToString(),
                    thread.ThreadState.ToString(),
                    thread.Site == null ? "Null" : thread.Site.Name,
                    thread.StartTime.ToLongTimeString(),
                    thread.GetLifetimeService() == null ? "Null" : thread.GetLifetimeService());

                Console.WriteLine(threadInfo);

                Console.WriteLine("---------------------");
            }

            Console.WriteLine("=====================");
        }
    }
}
