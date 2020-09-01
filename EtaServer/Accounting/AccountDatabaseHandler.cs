using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EtaServer
{
    /// <summary>
    /// Handles interactions with the MySql database used to store account information.
    /// </summary>
    internal class AccountDatabaseHandler
    {
        static string m_QueryAccountNameExistsString = 
            "select * from accounts where binary account_name = ?account limit 1";

        static string m_QueryAccountEmailExistsString =
            "select * from accounts where binary account_email = ?email limit 1";

        static string m_CreateAccountString = 
            "insert into accounts (id, account_name, pass_hash, pass_salt, account_email) " 
            + "values (?id, ?account, ?passHash, ?passSalt, ?email)";

        static string m_GetPasswordSaltString =
            "select pass_salt from accounts where account_name = ?account limit 1";

        static string m_GetPasswordHashString =
            "select pass_hash from accounts where account_name = ?account limit 1";

        static string m_GetAccountDetailsString = 
            "select id, user_rating, user_wins, user_losses, account_email from accounts where account_name = ?account limit 1";

        static string m_GetUserFriendsString =
            "select friendId from user_friendslist where userId = ?id";

        static string m_QueryFriendIdentityExistsString =
            "select userId from user_friendslist where userId = ?user and friendId = ?friend";

        static string m_AddUserFriendRelationshipString =
            "insert into user_friendslist (userId, friendId) values (?userId, ?friendId)";

        static string m_GetUserAccountFromIdString = 
            "select account_name from accounts where id = ?id limit 1";

        static string m_DeleteUserRelationshipString = 
            "delete from user_friendslist where userId = ?user and friendId = ?friend";

        static int m_HashSize = 16;

        internal static void CreateAccountFromConsole(string consoleInput)
        {
            string[] cmdSegments = consoleInput.Split(' ');

            if (cmdSegments.Length == 4)
                AttemptAccountCreation(string.Empty, cmdSegments[1], cmdSegments[2], cmdSegments[3]);
            else
                Console.WriteLine("[Error]: Invalid Parameters For Account Creation: (username, password, email).");
        }

        internal static void GetUserSaltFromConsole(string consoleInput)
        {
            string[] cmdSegments = consoleInput.Split(' ');

            if (cmdSegments.Length == 2)
                Console.WriteLine("Password Salt For: {0} [{1}]", cmdSegments[1], GetUserPassSalt(cmdSegments[1]));
            else
                Console.WriteLine("Invalid Parameter Count For Salt Acquisition.");
        }

        internal static void ChallengeAuthenticationFromConsole(string consoleInput)
        {
            string[] cmdSegments = consoleInput.Split(' ');

            if(cmdSegments.Length == 3)
            {
                if (ChallengeAccountAuthentication(string.Empty, cmdSegments[1], cmdSegments[2]))
                    Console.WriteLine("Client Authentication For [{0}] Successful.", cmdSegments[1]);
                else
                    Console.WriteLine("Client Authentication For [{0}] Failed.", cmdSegments[1]);
            }

            else
                Console.WriteLine("Invalid Parameter Count For Account Authentication: (username, password)");
        }

        internal static void AttemptLogin(string clientId, string username, string passwordAttempt)
        {
            if(ChallengeAccountAuthentication(clientId, username, passwordAttempt))
                NetworkEventDispatcher.InvokeUserLoginEvent(new UserLoginEventArgs(clientId, username, true, false));

            else
                NetworkEventDispatcher.InvokeUserLoginEvent(new UserLoginEventArgs(clientId, username, false, false));
        }

        internal static bool ChallengeAccountAuthentication(string clientid, string username, string passwordAttempt)
        {
            bool? accountExists = QueryAccountNameExists(username);

            if (accountExists == null)
            {
                Console.WriteLine("[Error]: Unable To Connect To Database For Query.");
                return false;
            }

            if (!accountExists.Value)
            {
                Console.WriteLine("[{0}] Failed To Login: Account Name ({1}) Does Not Exist.", clientid, username);
                return false;
            }

            string userPasswordSalt = GetUserPassSalt(username);
            string storedPasswordHash = GetUserPassHash(username);

            string passwordAttemptHash = GenerateHash(passwordAttempt + userPasswordSalt);

            if (userPasswordSalt != string.Empty 
                 && storedPasswordHash != string.Empty)
                    return passwordAttemptHash == storedPasswordHash;

            else
                Console.WriteLine("[Error]: Either Password Salt Or Hash Returned Empty On Authentication.");

            return false;
        }

        internal static string GetUserPassHash(string username)
        {
            try
            {
                using (MySqlConnection connection = MySqlConnector.InitializeMySqlConnection())
                {
                    if (MySqlConnector.OpenConnection(connection))
                    {
                        using (MySqlCommand c = new MySqlCommand(m_GetPasswordHashString, connection))
                        {
                            c.Parameters.Add(new MySqlParameter("?account", username));

                            using (MySqlDataReader reader = c.ExecuteReader())
                            {
                                if (reader.Read())
                                    return reader.GetString(0);

                                else
                                {
                                    Console.WriteLine("Unable To Obtain Password For Account: {0}", username);
                                    return string.Empty;
                                }
                            }
                        }
                    }

                    else
                    {
                        Console.WriteLine("[Error]: Unable To Contact Database During: (ChallengeUserPassword).");
                        return string.Empty;
                    }
                }
            }

            catch (Exception e) { Console.WriteLine(e.ToString()); return string.Empty; }
        }

        internal static string GetUserPassSalt(string username)
        {
            try
            {
                using (MySqlConnection connection = MySqlConnector.InitializeMySqlConnection())
                {
                    if(MySqlConnector.OpenConnection(connection))
                    {
                        using (MySqlCommand c = new MySqlCommand(m_GetPasswordSaltString, connection))
                        {
                            c.Parameters.Add(new MySqlParameter("?account", username));

                            using (MySqlDataReader reader = c.ExecuteReader())
                            {
                                if(reader.Read())
                                    return reader.GetString(0);

                                else
                                {
                                    Console.WriteLine("Unable To Obtain Hash Salt For Account: {0}", username);
                                    return string.Empty;
                                }
                            }
                        }
                    }

                    else
                    {
                        Console.WriteLine("[Error]: Unable To Contact Database During: (GetUserPassSalt).");
                        return string.Empty;
                    }
                }
            }

            catch(Exception e) { Console.WriteLine(e.ToString()); return string.Empty; }
        }

        internal static void AttemptAccountCreation
            (string clientId, string username, string password, string email)
        {
            bool? accountExists = QueryAccountExists(clientId, username, email);
            if (accountExists != null)
            {
                if (!accountExists.Value)
                {
                    string accountId = GenerateUniqueAccountId();
                    CreateAccount(clientId, accountId, username, password, email);
                }
            }

            else
                Console.WriteLine("[Error]: Unable To Contact Database During: (AttemptAccountCreation).");
        }

        internal static string GenerateUniqueAccountId()
        {
            string accountId = null;

            while(accountId == null || QueryAccountIdExists(accountId).Value)
                accountId = Guid.NewGuid().ToString();

            return accountId;
        }

        internal static void CreateAccount(string clientId, string accountId, string username, string password, string email)
        {
            string passwordSalt = GenerateHashSalt(m_HashSize);
            string passwordHash = GenerateHash(password + passwordSalt);
            try
            {
                using (MySqlConnection mySqlConnection = MySqlConnector.InitializeMySqlConnection())
                {
                    if (MySqlConnector.OpenConnection(mySqlConnection))
                    {
                        using (MySqlCommand c = new MySqlCommand(m_CreateAccountString, mySqlConnection))
                        {
                            c.Parameters.Add(new MySqlParameter("?id", accountId));
                            c.Parameters.Add(new MySqlParameter("?account", username));
                            c.Parameters.Add(new MySqlParameter("?passHash", passwordHash));
                            c.Parameters.Add(new MySqlParameter("?passSalt", passwordSalt));
                            c.Parameters.Add(new MySqlParameter("?email", email));

                            int i = c.ExecuteNonQuery();

                            if (i > 0)
                            {
                                if (clientId != string.Empty)
                                    NetworkEventDispatcher.InvokeAccountCreationSuccessEvent
                                        (new AccountCreationSuccessArgs(clientId));

                                Console.WriteLine("Account Successfully Created For: {0}", username);
                            }

                            else
                            {
                                if (clientId != string.Empty)
                                    NetworkEventDispatcher.InvokeAccountCreationFailedEvent
                                        (new AccountCreationFailedArgs(clientId, AccountCreationFailType.Unknown));

                                Console.WriteLine("Failed To Create New Account For: {0}", username);
                            }

                            MySqlConnector.CloseConnection(mySqlConnection);
                        }
                    }

                    else
                        Console.WriteLine("[Error]: Unable To Contact Database During: (CreateAccount).");
                }
            }

            catch(Exception e) { Console.WriteLine(e.ToString()); }
        }

        internal static bool? QueryAccountExists(string clientId, string username, string email)
        {
            try
            {
                using (MySqlConnection mySqlConnection = MySqlConnector.InitializeMySqlConnection())
                {
                    if (MySqlConnector.OpenConnection(mySqlConnection))
                    {
                        using (MySqlCommand c = new MySqlCommand(m_QueryAccountNameExistsString, mySqlConnection))
                        {
                            c.Parameters.Add(new MySqlParameter("?account", username));

                            using (MySqlDataReader reader = c.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if (clientId != string.Empty)
                                        NetworkEventDispatcher. //Notify Client Account Name Exists
                                            InvokeAccountCreationFailedEvent
                                                (new AccountCreationFailedArgs(clientId, AccountCreationFailType.UsernameExists));

                                    Console.WriteLine
                                        ("Failed To Create New Account For: {0} (Username [{1}] Already Exists)", 
                                            username, reader["account_name"].ToString());

                                    return true;
                                }
                            }
                        }

                        using (MySqlCommand c = new MySqlCommand(m_QueryAccountEmailExistsString, mySqlConnection))
                        {
                            c.Parameters.Add(new MySqlParameter("?email", email));

                            using (MySqlDataReader reader = c.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if (clientId != string.Empty)
                                        NetworkEventDispatcher. //Notify Client Email Already In Use
                                            InvokeAccountCreationFailedEvent
                                                (new AccountCreationFailedArgs(clientId, AccountCreationFailType.EmailExists));

                                    Console.WriteLine("Failed To Create New Account For: {0} (Email Already Exists)", username);

                                    return true;
                                }
                            }
                        }

                        MySqlConnector.CloseConnection(mySqlConnection);
                        return false;
                    }

                    return null; //Unable To Open Database Connection
                }
            }

            catch (Exception e) { Console.WriteLine(e.ToString()); return null; }
        }

        internal static bool? QueryAccountNameExists(string username)
        {
            try
            {
                using (MySqlConnection mySqlConnection = MySqlConnector.InitializeMySqlConnection())
                {
                    if (MySqlConnector.OpenConnection(mySqlConnection))
                    {
                        using (MySqlCommand c = new MySqlCommand(m_QueryAccountNameExistsString, mySqlConnection))
                        {
                            c.Parameters.Add(new MySqlParameter("?account", username));

                            using (MySqlDataReader reader = c.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    return true;
                                }
                            }
                        }

                        MySqlConnector.CloseConnection(mySqlConnection);
                        return false;
                    }

                    return null; //Unable To Open Database Connection
                }
            }

            catch (Exception e) { Console.WriteLine(e.ToString()); return null; }
        }

        internal static bool? QueryAccountIdExists(string accountId)
        {
            try
            {
                using (MySqlConnection mySqlConnection = MySqlConnector.InitializeMySqlConnection())
                {
                    if (MySqlConnector.OpenConnection(mySqlConnection))
                    {
                        using (MySqlCommand c = new MySqlCommand(m_GetUserAccountFromIdString, mySqlConnection))
                        {
                            c.Parameters.Add(new MySqlParameter("?id", accountId));

                            using (MySqlDataReader reader = c.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    return true;
                                }
                            }
                        }

                        MySqlConnector.CloseConnection(mySqlConnection);
                        return false;
                    }

                    return null; //Unable To Open Database Connection
                }
            }

            catch (Exception e) { Console.WriteLine(e.ToString()); return null; }
        }

        internal static string[] GetAccountDetails(string username)
        {
            string[] details = new string[5];

            try
            {
                using (MySqlConnection mySqlConnection = MySqlConnector.InitializeMySqlConnection())
                {
                    if (MySqlConnector.OpenConnection(mySqlConnection))
                    {
                        using (MySqlCommand c = new MySqlCommand(m_GetAccountDetailsString, mySqlConnection))
                        {
                            c.Parameters.Add(new MySqlParameter("?account", username));

                            using (MySqlDataReader reader = c.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    details[0] = reader["account_email"].ToString();
                                    details[1] = reader["user_rating"].ToString();
                                    details[2] = reader["user_wins"].ToString();
                                    details[3] = reader["user_losses"].ToString();
                                    details[4] = reader["id"].ToString();
                                }
                            }
                        }

                        MySqlConnector.CloseConnection(mySqlConnection);
                    }

                    else
                    {
                        Console.WriteLine("[Error]: Unable To Contact Database During: (GetAccountDetails).");
                        return null;
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }

            if (ServerCore.DebugMode)
                Console.WriteLine("Details For [{0}]: {1}, {2}, {3}, {4}, {5}", 
                    username, details[0], details[1], details[2], details[3], details[4]);

            return details;
        }

        internal static string[] GetUserFriends(string id)
        {
            List<string> friendIds = new List<string>();
            try
            {
                using (MySqlConnection mySqlConnection = MySqlConnector.InitializeMySqlConnection())
                {
                    if (MySqlConnector.OpenConnection(mySqlConnection))
                    {
                        using (MySqlCommand c = new MySqlCommand(m_GetUserFriendsString, mySqlConnection))
                        {
                            c.Parameters.Add(new MySqlParameter("?id", id));

                            using (MySqlDataReader reader = c.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    friendIds.Add((string)reader["friendId"]);
                                }
                            }
                        }

                        MySqlConnector.CloseConnection(mySqlConnection);
                    }

                    else
                    {
                        Console.WriteLine("[Error]: Unable To Contact Database During: (GetUserFriends).");
                        return null;
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }

            if (ServerCore.DebugMode)
                Console.WriteLine("Gathered [{0}] Friend Entries..", friendIds.Count);

            return friendIds.ToArray();
        }

        internal static bool? UserRelationshipExists(string userId, string friendId)
        {
            try
            {
                using (MySqlConnection mySqlConnection = MySqlConnector.InitializeMySqlConnection())
                {
                    if (MySqlConnector.OpenConnection(mySqlConnection))
                    {
                        using (MySqlCommand c = new MySqlCommand(m_QueryFriendIdentityExistsString, mySqlConnection))
                        {
                            c.Parameters.Add(new MySqlParameter("?user", userId));
                            c.Parameters.Add(new MySqlParameter("?friend", friendId));

                            using (MySqlDataReader reader = c.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if (ServerCore.DebugMode)
                                        Console.WriteLine("User Relationship [{1}] Does Not Exist For: {0}",
                                            AccountHandler.GetAccountById(userId), AccountHandler.GetAccountById(friendId));
                                    return true;
                                }

                                else 
                                    if (ServerCore.DebugMode)
                                        Console.WriteLine("User Relationship [{1}] Already Exists For: {0}",
                                            AccountHandler.GetAccountById(userId), AccountHandler.GetAccountById(friendId));
                            }
                        }

                        MySqlConnector.CloseConnection(mySqlConnection);
                    }

                    else
                    {
                        Console.WriteLine("[Error]: Unable To Contact Database During: (GetUserFriends).");
                        return null;
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }

            return false;
        }

        internal static void AddFriendRelationship(string userId, string friendId)
        {
            bool? relationshipExists = UserRelationshipExists(userId, friendId);
            if(relationshipExists != null && relationshipExists.Value == false)
            {
                try
                {
                    using (MySqlConnection mySqlConnection = MySqlConnector.InitializeMySqlConnection())
                    {
                        if (MySqlConnector.OpenConnection(mySqlConnection))
                        {
                            using (MySqlCommand c = new MySqlCommand(m_AddUserFriendRelationshipString, mySqlConnection))
                            {
                                c.Parameters.Add(new MySqlParameter("?userId", userId));
                                c.Parameters.Add(new MySqlParameter("?friendId", friendId));

                                int i = c.ExecuteNonQuery();

                                if (i > 0)
                                {
                                    Console.WriteLine("User Relationship Successfully Created For: {0}", AccountHandler.GetAccountById(userId).Username);
                                }

                                else
                                {
                                    Console.WriteLine("Failed To Create New User Relationship For: {0}", AccountHandler.GetAccountById(userId).Username);
                                }

                                MySqlConnector.CloseConnection(mySqlConnection);
                            }
                        }

                        else
                            Console.WriteLine("[Error]: Unable To Contact Database During: (CreateAccount).");
                    }
                }

                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }

        internal static string GetAccountNameFromId(string id)
        {
            string username = null;

            try
            {
                using (MySqlConnection mySqlConnection = MySqlConnector.InitializeMySqlConnection())
                {
                    if (MySqlConnector.OpenConnection(mySqlConnection))
                    {
                        using (MySqlCommand c = new MySqlCommand(m_GetUserAccountFromIdString, mySqlConnection))
                        {
                            c.Parameters.Add(new MySqlParameter("?id", id));

                            using (MySqlDataReader reader = c.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    username = reader["account_name"].ToString();
                                }
                            }
                        }

                        MySqlConnector.CloseConnection(mySqlConnection);
                    }

                    else
                    {
                        Console.WriteLine("[Error]: Unable To Contact Database During: (GetAccounFromId).");
                        return null;
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }

            return username;
        }

        internal static void RemoveUserRelationships(string user, string friend)
        {
            try
            {
                using (MySqlConnection mySqlConnection = MySqlConnector.InitializeMySqlConnection())
                {
                    if (MySqlConnector.OpenConnection(mySqlConnection))
                    {
                        using (MySqlCommand c = new MySqlCommand(m_DeleteUserRelationshipString, mySqlConnection))
                        {
                            c.Parameters.Add(new MySqlParameter("?user", user));
                            c.Parameters.Add(new MySqlParameter("?friend", friend));

                            int i = c.ExecuteNonQuery();

                            if (i > 0)
                            {
                                if(ServerCore.DebugMode)
                                    Console.WriteLine("Sucessfully Removed User Relationship: {0} <> {1}", user, friend);
                            }

                            else
                            {
                                if (ServerCore.DebugMode)
                                    Console.WriteLine("Failed To Remove User Relationship: {0} <> {1}", user, friend);
                            }

                            MySqlConnector.CloseConnection(mySqlConnection);
                        }
                    }

                    else
                        Console.WriteLine("[Error]: Unable To Contact Database During: (CreateAccount).");
                }
            }

            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        internal static string GenerateHashSalt(int saltSize)
        {
            try
            {
                RNGCryptoServiceProvider rcsp = new RNGCryptoServiceProvider();
                byte[] buffer = new byte[saltSize];

                rcsp.GetBytes(buffer);

                return Convert.ToBase64String(buffer);
            }

            catch (Exception e) { Console.WriteLine(e.ToString()); return null; }
        }

        internal static string GenerateHash(string input)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(input);

                SHA256Managed hashString = new SHA256Managed();
                byte[] hash = hashString.ComputeHash(buffer);

                return Convert.ToBase64String(buffer);
            }

            catch (Exception e) { Console.WriteLine(e.ToString()); return null; }
        }
    }
}