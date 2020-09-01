using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

static class DisableConsoleQuickEdit
{
    const uint ENABLE_QUICK_EDIT = 0x0040;

    // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
    const int STD_INPUT_HANDLE = -10;

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    internal static bool Go()
    {
        IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

        // get current console mode
        uint consoleMode;
        if (!GetConsoleMode(consoleHandle, out consoleMode))
        {
            // ERROR: Unable to get console mode.
            return false;
        }

        // Clear the quick edit bit in the mode flags
        consoleMode &= ~ENABLE_QUICK_EDIT;

        // set the new mode
        if (!SetConsoleMode(consoleHandle, consoleMode))
        {
            // ERROR: Unable to set console mode
            return false;
        }

        return true;
    }
}

namespace EtaServer
{
    class ServerCore
    {
        static SocketListener m_SocketListener;
        static MessageHandler m_MessageHandler;

        static string m_ServerCommand;
        static string m_TerminationCommand = "terminate";

        internal static bool DebugMode { get; set; }
        internal static bool RsaKeysLoaded { get; set; }

        static AesModule m_AesModule;

        static void Main(string[] args)
        {
            m_SocketListener = new SocketListener(11000);
            m_MessageHandler = new MessageHandler();

            if (DisableConsoleQuickEdit.Go())
                Console.WriteLine("Quick-Edit Mode Disabled..");

            InitializeModules();

            Task.Run(() => m_SocketListener.Start());
            Task.Run(() => ClientManager.CycleClientHeartbeats());

            LoopReadInput();
        }

        private static void InitializeModules()
        {
            RsaKeysLoaded = RSAModule.QueryRsa();

            m_AesModule = new AesModule();
            m_AesModule.InitializeProviderFromFile();

            MySqlConnector.InitializeMySqlPasswordFromEncrypted();
            MySqlConnection SqlConnection = MySqlConnector.InitializeMySqlConnection();

            if (SqlConnection != null)
            {
                SqlConnection.Dispose();
                Console.WriteLine
                    ("Mysql Database Connection Sucessful..");
            }
        }

        private static void LoopReadInput()
        {
            while ((m_ServerCommand = Console.ReadLine()).ToLowerInvariant() != m_TerminationCommand)
            {
                if (m_ServerCommand.IndexOf("decrsa") != -1)
                {
                    RSAModule.ParseDecryptCommand(m_ServerCommand);
                    continue;
                }

                else if (m_ServerCommand.IndexOf("encrsa") != -1)
                {
                    RSAModule.ParseEncryptCommand(m_ServerCommand);
                    continue;
                }

                else if (m_ServerCommand.IndexOf("decaes") != -1)
                {
                    m_AesModule.ParseDecryptCommand(m_ServerCommand);
                    continue;
                }

                else if (m_ServerCommand.IndexOf("encaes") != -1)
                {
                    m_AesModule.ParseEncryptCommand(m_ServerCommand);
                    continue;
                }

                else if (m_ServerCommand.IndexOf("crtacct") != -1)
                {
                    AccountDatabaseHandler.CreateAccountFromConsole(m_ServerCommand);
                    continue;
                }

                else if (m_ServerCommand.IndexOf("getsalt") != -1)
                {
                    AccountDatabaseHandler.GetUserSaltFromConsole(m_ServerCommand);
                    continue;
                }

                else if (m_ServerCommand.IndexOf("auth") != -1)
                {
                    AccountDatabaseHandler.ChallengeAuthenticationFromConsole(m_ServerCommand);
                    continue;
                }

                switch (m_ServerCommand.ToLowerInvariant())
                {
                    case "count":
                        {
                            Console.WriteLine
                                ("Number Of Authorized Clients Connected: {0}", ClientManager.ClientCount);
                            break;
                        }
                    case "debug":
                        {
                            if (DebugMode)
                            {
                                Console.WriteLine("Debug Mode Disabled.");
                                DebugMode = false;
                            }

                            else
                            {
                                Console.WriteLine("Debug Mode Enabled.");
                                DebugMode = true;
                            }
                            break;
                        }
                    case "genrsa":
                        {
                            Task.Run(() =>
                            {
                                RSAModule.GenerateNewRsaKeys();
                                Console.WriteLine("New Rsa Keys Generated..");
                            });

                            break;
                        }

                    case "outrsa":
                        {
                            Task.Run(() =>
                            {
                                RSAModule.OutputCurrentRsaKeys();
                                Console.WriteLine("Current Rsa Keys Cached..");
                            });
                            break;
                        }

                    case "genaes":
                        {
                            Task.Run(() =>
                            {
                                m_AesModule.GenerateProviderAndOutput();
                                Console.WriteLine("Aes Key And Vector Generated..");
                            });
                            break;
                        }
                    case "threadinfo":
                        {
                            Diagnostics.OutputThreadCountData();
                            break;
                        }
                    case "allthreads":
                        {
                            Diagnostics.OutputIndividualThreadData();
                            break;
                        }
                    default: Console.WriteLine("Invalid Command, Please Enter Valid Input."); break;
                }
            };
        }
    }
}
