using MonsterTradingCardsGame.MTCG_Models.Database;
using MonsterTradingCardsGame.MTCG_Models.Models;
using MonsterTradingCardsGame.MTCG_Models.Server;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Services.Authentication
{
    public class UserManagement
    {
        //private readonly NpgsqlConnection _connection;
        private readonly DatabaseConnection _dbConnection;
        private readonly TokenManagement _tokenManagement;
        private readonly PackageManagement _packageManagement;
        private readonly CardManagement _cardManagement;
        private Guid _userId;

        public UserManagement(DatabaseConnection dbConnection)
        {
            //_connection = connection;
            _dbConnection = dbConnection;
            _tokenManagement = new();
            _packageManagement = new(_dbConnection, 1);
            _cardManagement = new(_dbConnection);
        }

        //  Method that handles registering a new user
        public Response SignUp(Dictionary<string, string> requestBody)
        {
            using NpgsqlConnection connection = _dbConnection.OpenConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                string username = requestBody["Username"];
                string password = requestBody["Password"];

                if (!Parser.CheckIfValidString(username) || !Parser.CheckIfValidString(password))
                {
                    Console.WriteLine("Username or Password is empty.");
                    return new Response(400, "Username or Password is empty.");
                }

                if (connection == null || connection.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine($"Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }
                if (CheckIfUserExists(connection, username))
                {
                    Console.WriteLine("User already exists.");
                    return new Response(409, "User already exists.");
                }
                Console.WriteLine("Connection successfully established with Database.");

                using NpgsqlCommand command = new("INSERT INTO player (username, password) VALUES (@username, @password) RETURNING id", connection);

                command.Parameters.AddWithValue("username", username);
                command.Parameters.AddWithValue("password", password);
                _userId = (Guid)command.ExecuteScalar();

                if (_userId == Guid.Empty)
                {
                    transaction.Rollback();
                    Console.WriteLine("An error occured while retrieving user ID.");
                    return new Response(500, "Internal Server Error occured.");
                }

                Console.WriteLine($"{username} has been added to database!");

                //  Add default player stats to new user
                if (!AddPlayerStats(connection))
                {
                    transaction.Rollback();
                    Console.WriteLine($"Adding player stats to {username} with userID {_userId} failed.");
                    return new Response(500, "Internal Server Error occured.");
                }

                transaction.Commit();
                return new Response(201, $"{username} signed up successfully! Please login to proceed.");
            }
            catch (NpgsqlException e)
            {
                transaction.Rollback();
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "Internal Server Error occured.");
            }
            catch (Exception e)
            {
                transaction.Rollback();
                Console.WriteLine($"Error occurred while trying to signup user: {e.Message}");
                return new Response(500, "Internal Server Error occured.");
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
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while checking if user exists: {e.Message}");
                return false;
            }
        }

        public bool AddPlayerStats(NpgsqlConnection connection)
        {
            try
            {
                using NpgsqlCommand command = new("INSERT INTO player_stats (player_id) VALUES (@userId)", connection);
                command.Parameters.AddWithValue("userId", _userId);
                command.ExecuteNonQuery();

                Console.WriteLine($"Player stats has been added to UserID {_userId}.");
                return true;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while adding player stats to player: {e.Message}");
                return false;
            }
        }

        //  Method that handles login from already registered Users
        public Response Login(Dictionary<string, string> requestBody)
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                string username = requestBody["Username"];
                string password = requestBody["Password"];

                if (!Parser.CheckIfValidString(username) || !Parser.CheckIfValidString(password))
                {
                    Console.WriteLine("Username or Password is empty.");
                    return new Response(400, "Username or Password is empty.");
                }
                if (connection == null || connection.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }
                if (!CheckIfUserExists(connection, username))
                {
                    Console.WriteLine("User doesn't exist.");
                    return new Response(401, "Incorrect Username or Password.");
                }

                using NpgsqlCommand command = new("SELECT password FROM player WHERE username = @username", connection);
                command.Parameters.AddWithValue("username", username);
                string? result = command.ExecuteScalar()?.ToString();

                if (result == null || result != password)
                {
                    Console.WriteLine("Incorrect Username or Password.");
                    return new Response(401, "Incorrect Username or Password.");
                }

                Response response;
                string token = _tokenManagement.GenerateToken(connection, username);

                if (!Parser.CheckIfValidString(token))
                {
                    response = new(500, "Internal Server Error occured.");
                    return response;
                }

                Console.WriteLine($"{username} logged in with {token} successfully!");
                response = new(200, $"{username} logged in successfully!");
                response.SetToken(token);
                return response;

            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "Internal Server Error occured.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during login: {e.Message}");
                return new Response(500, "Internal Server Error occured.");
            }
        }

        public Response AquirePackage()
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null || connection.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }

                int userCoins = GetUserCoins(connection);
                if (userCoins < _packageManagement.GetPackageCost())
                {
                    Console.WriteLine($"Not enough money for buying a card package. Coins: {userCoins}");
                    return new Response(403, "Not enough money for buying a card package.");
                }

                //  Select random package if any exists
                Guid? randomPackageId = _packageManagement.GetRandomPackageId(connection);
                if (randomPackageId == null)
                {
                    Console.WriteLine($"No packages available for purchase.");
                    return new Response(404, "No card package available for buying.");
                }

                List<string> cardIds = _packageManagement.GetCardIds(connection, randomPackageId.Value);
                if (cardIds.Count < _packageManagement.GetPackageSize())
                {
                    Console.WriteLine($"Incorrect number of cards in package: {cardIds.Count}");
                    return new Response(500, "Internal Server Error occured.");
                }

                //  Delete the package and add the cards to the user stack
                using (var transaction = connection.BeginTransaction())
                {
                    if (!DeductCoins(connection, transaction, userCoins))
                    {
                        Console.WriteLine($"An error occured while buying the package.");
                        return new Response(500, "Internal Server Error occured.");
                    }

                    if (!_packageManagement.DeletePackage(connection, transaction, randomPackageId.Value))
                    {
                        transaction.Rollback();
                        Console.WriteLine("An error occured while deleting the aquired package.");
                        return new Response(500, "Internal Server Error occured.");
                    }

                    if (!_cardManagement.AddCardsToStack(_userId, cardIds, connection, transaction))
                    {
                        transaction.Rollback();
                        Console.WriteLine("An error occured while deleting the aquired package.");
                        return new Response(500, "Internal Server Error occured.");
                    }

                    transaction.Commit();
                }

                Console.WriteLine("Packages aquired successfully.");
                return new Response(201, "A package has been successfully bought.");
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "Internal Server Error occured.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during login: {e.Message}");
                return new Response(500, "Internal Server Error occured.");
            }
        }

        public bool DeductCoins(NpgsqlConnection connection, NpgsqlTransaction transaction, int userCoins)
        {
            try
            {
                int newCoins = userCoins - _packageManagement.GetPackageCost();
                using NpgsqlCommand command = new("UPDATE player_stats SET coins = @newCoins WHERE player_id = @userId", connection, transaction);
                command.Parameters.AddWithValue("newCoins", newCoins);
                command.Parameters.AddWithValue("userId", _userId);
                command.ExecuteNonQuery();

                return true;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during Admin Check: {e.Message}");
                return false;
            }
        }

        //  Missing Implementation
        public Response GetUserData(Dictionary<string, string> requestBody, string token, string username)
        {
            using NpgsqlConnection connection = _dbConnection.OpenConnection();
            if (connection == null || connection.State != System.Data.ConnectionState.Open)
            {
                Console.WriteLine("Connection to Database failed.");
                return new Response(500, "Internal Server Error occured.");
            }

            if (!CheckIfTokenIsValid(token))
            {
                return new Response(401, "Unauthorized.");
            }

            return new Response(0, "");
        }

        //  Missing Implementation
        public Response UpdateUserData(Dictionary<string, string> requestBody, string token)
        {
            using NpgsqlConnection connection = _dbConnection.OpenConnection();
            if (connection == null || connection.State != System.Data.ConnectionState.Open)
            {
                Console.WriteLine("Connection to Database failed.");
                return new Response(500, "Internal Server Error occured.");
            }

            if (!CheckIfTokenIsValid(token))
            {
                return new Response(401, "Unauthorized.");
            }

            return new Response(0, "");
        }

        //  Helper method that checks if a user has admin priviledges
        public bool CheckIfAdmin(NpgsqlConnection connection, string token)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT token FROM player WHERE username = @username", connection);
                command.Parameters.AddWithValue("@username", "admin");

                string? resultToken = command.ExecuteScalar()?.ToString();

                if (resultToken == null || resultToken != token)
                {
                    return false;
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
                Console.WriteLine($"Error occured during Admin Check: {e.Message}");
                return false;
            }
        }

        public Guid? GetUserId(NpgsqlConnection connection, string token)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT id FROM player WHERE token = @token", connection);
                command.Parameters.AddWithValue("token", token);
                _userId = (Guid)command.ExecuteScalar();

                return _userId;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while checking for user ID: {e.Message}");
                return null;
            }
        }

        public string? GetUsername(NpgsqlConnection connection, string token)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT username FROM player WHERE token = @token", connection);
                command.Parameters.AddWithValue("token", token);

                return command.ExecuteScalar()?.ToString();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during Admin Check: {e.Message}");
                return null;
            }
        }

        public int GetUserCoins(NpgsqlConnection connection)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT coins FROM player_stats WHERE player_id = @userId", connection);
                command.Parameters.AddWithValue("userId", _userId);

                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during Admin Check: {e.Message}");
                return 0;
            }
        }

        public Response GetUserStack(string token)
        {
            try
            {
                NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null || connection.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }

                Guid? userId = GetUserId(connection, token);
                if (userId == null)
                {
                    Console.WriteLine("User ID is null.");
                    return new Response(500, "Internal Server Error occured.");
                }

                List<Card> stack = _cardManagement.GetStack(connection, userId.Value);
                if(stack == null || stack.Count <= 0)
                {
                    Console.WriteLine("Stack is empty");
                }

                return new Response(200, stack);
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("Connection to Database failed.");
                return new Response(500, "Internal Server Error occured.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured while retrieving user stack.");
                return new Response(500, "Internal Server Error occured.");
            }            
        }

        public Response GetUserDeck(string token)
        {
            try
            {
                NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null || connection.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }

                Guid? userId = GetUserId(connection, token);
                if (userId == null)
                {
                    Console.WriteLine("User ID is null.");
                    return new Response(500, "Internal Server Error occured.");
                }

                List<Card> deck = _cardManagement.GetDeck(connection, userId.Value);
                if (deck == null || deck.Count <= 0)
                {
                    Console.WriteLine("Stack is empty");
                }

                return new Response(200, deck);
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("Connection to Database failed.");
                return new Response(500, "Internal Server Error occured.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured while retrieving user deck.");
                return new Response(500, "Internal Server Error occured.");
            }
        }


        public bool CheckIfTokenIsValid(string token)
        {
            using NpgsqlConnection connection = _dbConnection.OpenConnection();
            return _tokenManagement.CheckIfTokenIsValid(connection, token);
        }
    }
}
