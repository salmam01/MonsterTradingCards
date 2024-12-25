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
        private readonly DatabaseConnection _dbConnection;
        private TokenManagement _tokenManagement;

        public UserManagement()
        {
            _dbConnection = new DatabaseConnection();
            _tokenManagement = new TokenManagement();
        }

        //  Method that handles registering a new user
        public (int, string) SignUp(Dictionary<string, string> requestBody)
        {
            try
            {
                string username = requestBody["Username"];
                string password = requestBody["Password"];

                //  Can make this a method
                if (!Parser.CheckIfValidString(username) || !Parser.CheckIfValidString(password))
                {
                    Console.WriteLine("Username or Password is empty.");
                    return (400, "Username or Password is empty.");
                }

                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null)
                {
                    Console.WriteLine($"Connection failed. Status: {connection.State}");
                    return (500, "Internal Server Error occured.");
                }
                if (CheckIfUserExists(connection, username))
                {
                    Console.WriteLine("User already exists.");
                    return (409, "User already exists.");
                }
                Console.WriteLine("Connection successfully established with Database.");

                using NpgsqlCommand command = new("INSERT INTO player (username, password) VALUES (@username, @password)", connection);

                command.Parameters.AddWithValue("username", username);
                command.Parameters.AddWithValue("password", password);
                command.ExecuteNonQuery();
                Console.WriteLine($"{username} has been added to database!");
                
                return (201, $"{username} signed up successfully! Please login to proceed.");
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return (500, "Internal Server Error occured.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred while trying to signup user: {e.Message}");
                return (500, "Internal Server Error occured.");
            }
        }

        //  Helper method to check if a username is in the database
        public bool CheckIfUserExists(NpgsqlConnection connection, string username)
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
        public (int, string, string) Login(Dictionary<string, string> requestBody)
        {
            string username = requestBody["Username"];
            string password = requestBody["Password"];

            if (!Parser.CheckIfValidString(username) || !Parser.CheckIfValidString(password))
            {
                Console.WriteLine("Username or Password is empty.");
                return (400, "Username or Password is empty.", "");
            }
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null)
                {
                    Console.WriteLine($"Connection failed. Status: {connection.State}");
                    return (500, "Internal Server Error occured.", "");
                }
                if (!CheckIfUserExists(connection, username))
                {
                    Console.WriteLine("User doesn't exist.");
                    return (401, "Incorrect Username or Password.", "");
                }

                using NpgsqlCommand command = new("SELECT password FROM player WHERE username = @username", connection);
                command.Parameters.AddWithValue("username", username);
                object resultObj = command.ExecuteScalar();

                if (resultObj == null)
                {
                    return (401, "Incorrect Username or Password.", "");
                }

                string result = resultObj?.ToString();

                if (result == password)
                {
                    string token = _tokenManagement.GenerateToken(username);
                    Console.WriteLine($"Token: {token}");

                    if (!Parser.CheckIfValidString(token))
                    {
                        return (500, "Internal Server Error occured.", "");
                    }

                    Console.WriteLine($"{username} logged in with {token} successfully!");
                    return (200, $"{username} logged in successfully!", token);
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return (500, "Internal Server Error occured.", "");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during login: {e.Message}");
                return (500, "Internal Server Error occured.", "");
            }

            return (401, "Incorrect Username or Password.", "");
        }

        public (int, string) CreatePackages(Dictionary<string, string> requestBody)
        {
            if (!AdminCheck(requestBody["Username"]))
            {
                return (409, "Provided user is not admin.");
            }

            return (201, "Package and cards successfully created.");
        }

        //  Helper method that checks if a user has admin priviledges
        public bool AdminCheck(string username)
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
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
                return false;
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error occured during Admin Check: {e.Message}");
                return false;
            }

        }



    }
}
