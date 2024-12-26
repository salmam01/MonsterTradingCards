using MonsterTradingCardsGame.MTCG_Models.Database;
using MonsterTradingCardsGame.MTCG_Models.Models;
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

        // SHOULD RETURN RESPONSE OBJECT REALISTICALLY

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
                    Console.WriteLine($"Connection to Database failed.");
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
                    Console.WriteLine("Connection to Database failed.");
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
                    string token = _tokenManagement.GenerateToken(connection, username);

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

        public (int, string) CreatePackage(List<Card> requestBody, string token)
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return (500, "Internal Server Error occured.");
                }

                if (!CheckIfTokenIsValid(connection, token))
                {
                    return (401, "Unauthorized.");
                }
                if (!AdminCheck(connection, token))
                {
                    return (409, "Provided user is not admin.");
                }

                //  Add cards to the database
                if(!AddCardToDatabase(connection, requestBody))
                {
                    return (500, "An internal server error occurred.");
                }

                //  Add cards to package
                int shopId = 1;
                for (int i = 0; i < requestBody.Count; i++)
                {
                    using NpgsqlCommand command = new("INSERT INTO package (shop_id, card_id) VALUES (@shopId, @cardId)", connection);
                    command.Parameters.AddWithValue("shopId", shopId);
                    command.Parameters.AddWithValue("cardId", requestBody[i].Id);
                    command.ExecuteNonQuery();
            
                    Console.WriteLine($"{requestBody[i].Name} has been added to package!");
                }

                return (201, "Package and cards successfully created.");
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return (500, "Internal Server Error occured.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during login: {e.Message}");
                return (500, "Internal Server Error occured.");
            }
        }

        public bool AddCardToDatabase(NpgsqlConnection connection, List<Card> requestBody)
        {
            try
            {
                for (int i = 0; i < requestBody.Count; i++)
                {
                    using NpgsqlCommand command = new("INSERT INTO card (id, name, damage) VALUES (@id, @name, @damage)", connection);
                    command.Parameters.AddWithValue("id", requestBody[i].Id);
                    command.Parameters.AddWithValue("name", requestBody[i].Name);
                    command.Parameters.AddWithValue("damage", requestBody[i].Damage);
                    command.ExecuteNonQuery();

                    Console.WriteLine($"{requestBody[i].Name} has been added to database!");
                }
                return true;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during login: {e.Message}");
                return false;
            }

        }

        //  Missing Implementation
        public (int, string) GetUserData(Dictionary<string, string> requestBody, string token)
        {
            using NpgsqlConnection connection = _dbConnection.OpenConnection();
            if (connection == null)
            {
                Console.WriteLine("Connection to Database failed.");
                return (500, "Internal Server Error occured.");
            }

            if (!CheckIfTokenIsValid(connection, token))
            {
                return (401, "Unauthorized.");
            }

            return (0, "");
        }

        //  Missing Implementation
        public (int, string) UpdateUserData(Dictionary<string, string> requestBody, string token)
        {
            using NpgsqlConnection connection = _dbConnection.OpenConnection();
            if (connection == null)
            {
                Console.WriteLine("Connection to Database failed.");
                return (500, "Internal Server Error occured.");
            }

            if (!CheckIfTokenIsValid(connection, token))
            {
                return (401, "Unauthorized.");
            }

            return (0, "");
        }

        public bool CheckIfTokenIsValid(NpgsqlConnection connection, string token)
        {
            return (_tokenManagement.CheckIfTokenIsValid(connection, token)) ? true : false;
        }

        //  Helper method that checks if a user has admin priviledges
        public bool AdminCheck(NpgsqlConnection connection, string token)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT token FROM player WHERE username = @username", connection);
                command.Parameters.AddWithValue("@username", "admin");

                var resultObj = command.ExecuteScalar();

                if (resultObj == null || (resultObj.ToString() != token))
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
