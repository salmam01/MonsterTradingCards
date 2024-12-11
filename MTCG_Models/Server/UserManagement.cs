using MonsterTradingCardsGame.MTCG_Models.Database;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class UserManagement
    {
        //  Method that handles registering a new user
        public static int Register(Dictionary<string, string> request)
        {
            try
            {
                string username = request["Username"];
                string password = request["Password"];

                //  Can make this a method
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Username or Password are empty.");
                    return 400;
                }

                using NpgsqlConnection connection = DatabaseConnection.ConnectToDatabase();
                if (connection == null)
                {
                    Console.WriteLine($"Connection failed. Status: {connection.State}");
                    return 500;
                }
                if (CheckIfUserExists(connection, username))
                {
                    Console.WriteLine("User already exists.");
                    return 409;
                }
                Console.WriteLine("Connection successfully established with Database.");

                using NpgsqlCommand command = new("INSERT INTO player (username, password) VALUES (@username, @password)", connection);

                command.Parameters.AddWithValue("username", username);
                command.Parameters.AddWithValue("password", password);
                command.ExecuteNonQuery();
                Console.WriteLine($"{username} has been added to database!");
                
                return 201;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return 500;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred while trying to signup user: {e.Message}");
                return 500;
            }
        }

        public static bool CheckIfUserExists(NpgsqlConnection connection, string username)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT COUNT(*) FROM player WHERE username = @username", connection);
                command.Parameters.AddWithValue("username", username);

                //  Check if the username already exists, UNIQUE constraint is set so it should never be > 1
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while checking for user: {e.Message}");
                throw;
            }
        }

        //  Method that handles login from already registered Users
        public static int Login(Dictionary<string, string> data)
        {
            string username = data["Username"];
            string password = data["Password"];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine($"Wrong Username or Password: {username}");
                return 400;
            }
            try
            {
                using NpgsqlConnection connection = DatabaseConnection.ConnectToDatabase();
                if (connection == null)
                {
                    Console.WriteLine($"Connection failed. Status: {connection.State}");
                    return 500;
                }
                if (!CheckIfUserExists(connection, username))
                {
                    Console.WriteLine("User doesn't exist.");
                    return 404;
                }

                using NpgsqlCommand command = new("SELECT password FROM player WHERE username = @username", connection);
                command.Parameters.AddWithValue("username", username);
                object resultObj = command.ExecuteScalar();

                if (resultObj == null)
                {
                    return 404;
                }

                string result = resultObj?.ToString();

                if (result == password)
                {
                    string token = TokenManagement.GenerateToken(username);

                    if (token == null)
                    {
                        return 401;
                    }

                    Console.WriteLine($"{username} logged in with {token} successfully!");
                    return 200;
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return 500;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during login: {e.Message}");
                return 500;
            }

            return 401;
        }

        //  Helper method that checks if a user has admin priviledges
        public static bool AdminCheck(string username)
        {
            try
            {
                using NpgsqlConnection connection = DatabaseConnection.ConnectToDatabase();
                if (connection == null)
                {
                    Console.WriteLine($"Connection failed. Status: {connection.State}");
                    return false;
                }

                using NpgsqlCommand command = new("SELECT @username FROM player WHERE @username = admin");
                command.Parameters.AddWithValue("username", username);
                object resultObj = command.ExecuteScalar();

                if (resultObj == null)
                {
                    return false;
                }
                return true;
            }
            catch(NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return 500;
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error occured during Admin Check: {e.Message}");
                return 500;
            }

        }

    }
}
