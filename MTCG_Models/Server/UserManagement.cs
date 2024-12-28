using MonsterTradingCardsGame.MTCG_Models.Database;
using MonsterTradingCardsGame.MTCG_Models.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class UserManagement
    {
        private readonly DatabaseConnection _dbConnection;
        private TokenManagement _tokenManagement;
        private const int _packageSize = 5;
        private const int _packageCost = 5;
        private const int _shopId = 1;
        public UserManagement()
        {
            _dbConnection = new DatabaseConnection();
            _tokenManagement = new TokenManagement();
        }

        //  Method that handles registering a new user
        public Response SignUp(Dictionary<string, string> requestBody)
        {
            try
            {
                string username = requestBody["Username"];
                string password = requestBody["Password"];

                //  Can make this a method
                if (!Parser.CheckIfValidString(username) || !Parser.CheckIfValidString(password))
                {
                    Console.WriteLine("Username or Password is empty.");
                    return new Response(400, "Username or Password is empty.");
                }

                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null)
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

                using NpgsqlCommand command = new("INSERT INTO player (username, password) VALUES (@username, @password)", connection);

                command.Parameters.AddWithValue("username", username);
                command.Parameters.AddWithValue("password", password);
                command.ExecuteNonQuery();
                Console.WriteLine($"{username} has been added to database!");
                
                //  Add default player stats to new user
                Guid? userId = GetUserId(connection, username);
                if(userId == null)
                {
                    Console.WriteLine("User ID doesn't exist.");
                    return new Response(500, "Internal Server Error occured.");
                }

                if(!AddPlayerStats(connection, userId.Value))
                {
                    Console.WriteLine($"Adding player stats to {username} with userID {userId} failed.");
                    return new Response(500, "Internal Server Error occured.");
                }

                return new Response(201, $"{username} signed up successfully! Please login to proceed.");
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "Internal Server Error occured.");
            }
            catch (Exception e)
            {
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
                Console.WriteLine($"Error while checking for user: {e.Message}");
                return false;
            }
        }

        public bool AddPlayerStats(NpgsqlConnection connection, Guid userId)
        {
            try
            {
                using NpgsqlCommand command = new("INSERT INTO player_stats (player_id) VALUES (@userId)", connection);
                command.Parameters.AddWithValue("userId", userId);
                command.ExecuteNonQuery();

                Console.WriteLine($"Player_stats has been added to UserID {userId}.");
                return true;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while checking for user: {e.Message}");
                return false;
            }
        }

        //  Method that handles login from already registered Users
        public Response Login(Dictionary<string, string> requestBody)
        {
            try
            {
                string username = requestBody["Username"];
                string password = requestBody["Password"];

                if (!Parser.CheckIfValidString(username) || !Parser.CheckIfValidString(password))
                {
                    Console.WriteLine("Username or Password is empty.");
                    return new Response(400, "Username or Password is empty.");
                }
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null)
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
                object resultObj = command.ExecuteScalar();

                if (resultObj == null)
                {
                    return new Response(401, "Incorrect Username or Password.");
                }

                string result = resultObj?.ToString();

                if (result == password)
                {
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
                else
                {
                    return new Response(401, "Incorrect Username or Password.");
                }
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

        public Response CreatePackage(List<Card> cards, string token)
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }
                if (!CheckIfTokenIsValid(connection, token))
                {
                    return new Response(401, "Unauthorized.");
                }
                if (!AdminCheck(connection, token))
                {
                    return new Response(409, "Provided user is not admin.");
                }
                if(cards.Count != 5)
                {
                    return new Response(403, "A package can only contain 5 cards..");
                }
 
                //  Create a new package
                int shopId = 1;
                using NpgsqlCommand command = new("INSERT INTO package (shop_id) VALUES (@shopId) RETURNING id", connection);
                command.Parameters.AddWithValue("shopId", shopId);

                //  Retrieve the new package id
                string? packageId = command.ExecuteScalar()?.ToString();
                if(packageId == null)
                {
                    Console.WriteLine("Error occured while retrieving package ID.");
                    return new Response(500, "An internal server error occurred.");
                }
                Console.WriteLine($"Package with ID {packageId} has been added to the database.");

                //  Add cards to the database
                if(!AddCardsToDatabase(connection, cards, packageId))
                {
                    return new Response(500, "An internal server error occurred.");
                }

                Console.WriteLine($"Cards have been added to the package.");
                return new Response(201, "Package and cards successfully created.");
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

        public bool AddCardsToDatabase(NpgsqlConnection connection, List<Card> cards, string packageId)
        {
            try
            {
                // Convert user ID to a UUID
                if (!Guid.TryParse(packageId, out Guid packageGuid))
                {
                    Console.WriteLine($"Invalid package ID: {packageId}");
                    return false;
                }

                for (int i = 0; i < cards.Count; i++)
                {
                    using NpgsqlCommand command = new("INSERT INTO card (id, name, damage, package_id) VALUES (@id, @name, @damage, @packageId)", connection);
                    command.Parameters.AddWithValue("id", cards[i].Id);
                    command.Parameters.AddWithValue("name", cards[i].Name);
                    command.Parameters.AddWithValue("damage", cards[i].Damage);
                    command.Parameters.AddWithValue("packageId", packageGuid);
                    command.ExecuteNonQuery();

                    Console.WriteLine($"{cards[i].Name} has been added to database!");
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

        public Response AquirePackage(string token)
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }

                if (!CheckIfTokenIsValid(connection, token))
                {
                    Console.WriteLine("Token invalid.");
                    return new Response(401, "Unauthorized.");
                }

                string? username = GetUsername(connection, token);
                if(username == null)
                {
                    Console.WriteLine("Username is null.");
                    return new Response(500, "Internal Server Error occured.");
                }

                Guid? userId = GetUserId(connection, username);
                if (userId == null)
                {
                    Console.WriteLine("User ID is null.");
                    return new Response(500, "Internal Server Error occured.");
                }

                int coins = GetUserCoins(connection, userId);
                if(coins < _packageCost)
                {
                    Console.WriteLine($"Not enough money for buying a card package. Coins: {coins}");
                    return new Response(403, "Not enough money for buying a card package.");
                }

                //  Select random package if any exists
                Guid? randomPackageId = GetRandomPackage(connection);
                if (randomPackageId == null)
                {
                    Console.WriteLine($"No packages available for purchase.");
                    return new Response(404, "No card package available for buying.");
                }

                List<string> cardIds = new();

                // Make sure the SELECT query completes before moving forward
                using (NpgsqlConnection selectConnection = _dbConnection.OpenConnection())
                {
                    using NpgsqlCommand command = new("SELECT id FROM card WHERE package_id = @packageId", selectConnection);
                    command.Parameters.AddWithValue("packageId", randomPackageId);

                    using NpgsqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        cardIds.Add(reader["id"].ToString());
                    }
                }

                if (cardIds.Count < 5)
                {
                    Console.WriteLine($"Incorrect number of cards in package: {cardIds.Count}");
                    return new Response(500, "Internal Server Error occured.");
                }

                //  Delete the package once done
                if (!DeletePackage(connection, randomPackageId.Value))
                {
                    Console.WriteLine("An error occured while deleting the aquired package.");
                    return new Response(500, "Internal Server Error occured.");
                }

                Console.WriteLine("Packages aquired successfully.");
                return new Response(200, "A package has been successfully bought.");
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

        public bool DeletePackage(NpgsqlConnection connection, Guid packageId)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using NpgsqlCommand deletePackageId = new("UPDATE card SET package_id = NULL WHERE package_id = @packageId", connection, transaction);
                    deletePackageId.Parameters.AddWithValue("packageId", packageId);
                    deletePackageId.ExecuteNonQuery();

                    using NpgsqlCommand deletePackage = new("DELETE FROM package WHERE id = @packageId", connection, transaction);
                    deletePackage.Parameters.AddWithValue("packageId", packageId);
                    deletePackage.ExecuteNonQuery();

                    transaction.Commit();
                    return true; 
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error while deleting package: {e.Message}");
                    return false;
                }
            }
        }

        //  Missing Implementation
        public Response GetUserData(Dictionary<string, string> requestBody, string token, string username)
        {
            using NpgsqlConnection connection = _dbConnection.OpenConnection();
            if (connection == null)
            {
                Console.WriteLine("Connection to Database failed.");
                return new Response(500, "Internal Server Error occured.");
            }

            if (!CheckIfTokenIsValid(connection, token))
            {
                return new Response(401, "Unauthorized.");
            }

            return new Response(0, "");
        }

        //  Missing Implementation
        public Response UpdateUserData(Dictionary<string, string> requestBody, string token)
        {
            using NpgsqlConnection connection = _dbConnection.OpenConnection();
            if (connection == null)
            {
                Console.WriteLine("Connection to Database failed.");
                return new Response(500, "Internal Server Error occured.");
            }

            if (!CheckIfTokenIsValid(connection, token))
            {
                return new Response(401, "Unauthorized.");
            }

            return new Response(0, "");
        }

        public Guid? GetUserId(NpgsqlConnection connection, string username)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT id FROM player WHERE username = @username", connection);
                command.Parameters.AddWithValue("username", username);
                string? userId = command.ExecuteScalar()?.ToString();

                if (userId == null)
                {
                    return null;
                }

                // Convert user ID to a UUID
                if (!Guid.TryParse(userId, out Guid playerGuid))
                {
                    Console.WriteLine($"Invalid player ID: {userId}");
                    return null;
                }

                return playerGuid;
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

        public int GetUserCoins(NpgsqlConnection connection, Guid? userId)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT coins FROM player_stats WHERE player_id = @userId", connection);
                command.Parameters.AddWithValue("userId", userId);

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

        public Guid? GetRandomPackage(NpgsqlConnection connection)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT id FROM package WHERE shop_id = @shopId ORDER BY RANDOM() LIMIT 1", connection);
                command.Parameters.AddWithValue("shopId", _shopId);
                string? randomId = command.ExecuteScalar()?.ToString();

                if (randomId == null)
                {
                    return null;
                }

                // Convert user ID to a UUID
                if (!Guid.TryParse(randomId, out Guid randomGuid))
                {
                    Console.WriteLine($"Invalid player ID: {randomId}");
                    return null;
                }

                return randomGuid;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during package counting: {e.Message}");
                return null;
            }
        }

        public bool CheckIfTokenIsValid(NpgsqlConnection connection, string token)
        {
            return _tokenManagement.CheckIfTokenIsValid(connection, token);
        }

        //  Helper method that checks if a user has admin priviledges
        public bool AdminCheck(NpgsqlConnection connection, string token)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT token FROM player WHERE username = @username", connection);
                command.Parameters.AddWithValue("@username", "admin");

                object resultObj = command.ExecuteScalar();

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
