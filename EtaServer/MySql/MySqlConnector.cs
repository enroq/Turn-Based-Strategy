using MySql.Data.MySqlClient;
using System;

namespace EtaServer
{
    internal class MySqlConnector
    {
        private static readonly string server = "127.0.0.1";
        private static readonly string database = "eta_schema";
        private static readonly string uid = "manager";
        private static readonly string password =
            "qRDhgS9dHAgI57E6FTiX6Ic+Gm4ZIw3vi6ntsVhSvNd3U/Ri33VmO+uCkcxCn6HPYSwcE7x1LJCobe0" 
          + "/3jqfX1J8I1GwYGEN+HTKGbeniGLoTqOLTWf9xSp7dQ439bHIenI3DfLc1nGgVYMuJZMlpw53N3OqsGhC/LlLsF71KtY=";

        private static string connectionData;

        internal static string ConnectionData { get { return connectionData; } }

        internal static void InitializeMySqlPasswordFromEncrypted()
        {
            try
            {
                string decryptedPassword = RSAModule.DecryptStringToString(password);

                Console.WriteLine("Mysql Database Password Decrypted..");

                connectionData = "SERVER=" + server + ";" + "DATABASE=" +
                    database + ";" + "UID=" + uid + ";" + "PASSWORD=" + decryptedPassword + ";";
            }

            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        internal static MySqlConnection InitializeMySqlConnection()
        {
            MySqlConnection connection;
            try
            {
                connection = new MySqlConnection(connectionData);

                connection.Open();
                connection.Close();
            }

            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }

            return connection;
        }

        internal static bool OpenConnection(MySqlConnection connection)
        {
            try
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    return true;

                connection.Open();
                return true;
            }

            catch(Exception e) { Console.WriteLine(e.ToString()); return false; }
        }

        internal static bool CloseConnection(MySqlConnection connection)
        {
            try
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                    return true;

                connection.Close();
                return true;
            }

            catch(Exception e) { Console.WriteLine(e.ToString()); return false; }
        }
    }
}
