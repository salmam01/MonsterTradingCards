﻿using MonsterTradingCardsGame.Database;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Services;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MonsterTradingCardsGame.Services.Authentication
{
    public class UserManagement
    {
        //private readonly NpgsqlConnection _connection;
        private readonly DatabaseConnection _dbConnection;
        private readonly TokenManagement _tokenManagement;
        private readonly PackageManagement _packageManagement;
        private readonly CardManagement _cardManagement;

        public UserManagement(DatabaseConnection dbConnection, int shopId)
        {
            _dbConnection = dbConnection;
            _tokenManagement = new();
            _packageManagement = new(_dbConnection, shopId);
            _cardManagement = new();
        }

        //  Method that handles registering a new user
        public Response SignUp(Dictionary<string, string> requestBody)
        {
            using NpgsqlConnection connection = _dbConnection.OpenConnection();
            using (NpgsqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    if (connection == null || connection.State != ConnectionState.Open)
                    {
                        Console.WriteLine($"Connection to Database failed.");
                        return new Response(500, "An internal server error occurred.");
                    }

                    string username = requestBody["Username"];
                    string password = requestBody["Password"];

                    if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username) || username.Length <= 0)
                    {
                        Console.WriteLine("Username or Password is empty.");
                        return new Response(400, "Username cannot be empty.");
                    }
                    if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password) || password.Length <= 0)
                    {
                        Console.WriteLine("Username or Password is empty.");
                        return new Response(400, "Password cannot be empty.");
                    }

                    if (CheckIfUserExists(connection, username))
                    {
                        Console.WriteLine("User already exists.");
                        return new Response(409, "User already exists.");
                    }
                    Console.WriteLine("Connection successfully established with Database.");

                    using NpgsqlCommand command = new("INSERT INTO users (username, password) VALUES (@username, @password) RETURNING id", connection, transaction);

                    command.Parameters.AddWithValue("username", username);
                    command.Parameters.AddWithValue("password", password);
                    Guid? userId = (Guid?)command.ExecuteScalar();

                    if (userId == Guid.Empty || userId == null)
                    {
                        transaction.Rollback();
                        Console.WriteLine("An error occured while retrieving user ID.");
                        return new Response(500, "Internal Server Error occured.");
                    }

                    Console.WriteLine($"{username} has been added to database!");

                    //  Add default user stats to new user
                    if (!AddUserStats(connection, transaction, userId.Value))
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Adding user stats to {username} with userID {userId} failed.");
                        return new Response(500, "Internal Server Error occured.");
                    }

                    transaction.Commit();
                    return new Response(201, $"{username} signed up successfully! Please login to proceed.");
                }
                catch (NpgsqlException e)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Failed to connect to Database: {e.Message}");
                    return new Response(500, "An internal server error occurred.");
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Console.WriteLine($"An error occurred during signup: {e.Message}");
                    return new Response(500, "An internal server error occurred.");
                }
            }
        }

        public bool AddUserStats(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid userId)
        {
            using NpgsqlCommand command = new("INSERT INTO user_stats (user_id) VALUES (@userId)", connection, transaction);
            command.Parameters.AddWithValue("userId", userId);
            return command.ExecuteNonQuery() > 0;
        }

        //  Method that handles login from already registered Users
        public Response Login(Dictionary<string, string> requestBody)
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null || connection.State != ConnectionState.Open)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }

                string username = requestBody["Username"];
                string password = requestBody["Password"];

                if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username) || username.Length <= 0)
                {
                    Console.WriteLine("Username or Password is empty.");
                    return new Response(400, "Username cannot be empty.");
                }
                if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password) || password.Length <= 0)
                {
                    Console.WriteLine("Username or Password is empty.");
                    return new Response(400, "Password cannot be empty.");
                }

                if (!CheckIfUserExists(connection, username))
                {
                    Console.WriteLine("User not found.");
                    return new Response(401, "Incorrect Username or Password.");
                }

                using NpgsqlCommand command = new("SELECT password FROM users WHERE username = @username", connection);
                command.Parameters.AddWithValue("username", username);
                string? result = command.ExecuteScalar()?.ToString();

                if (result == null || result != password)
                {
                    Console.WriteLine("Incorrect Username or Password.");
                    return new Response(401, "Incorrect Username or Password.");
                }

                Response response;
                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    string? token = _tokenManagement.CreateToken(connection, transaction, username);

                    if (token == null)
                    {
                        response = new(500, "Internal Server Error occured.");
                        transaction.Rollback();
                        return response;
                    }

                    Console.WriteLine($"{username} logged in with {token} successfully!");
                    response = new(200, $"{username} logged in successfully!");
                    response.SetToken(token);
                    transaction.Commit();
                }
                return response;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred during login: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
        }

        public Response AquirePackage(string token)
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null || connection.State != ConnectionState.Open)
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

                int userCoins = GetUserCoins(connection, userId.Value);
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
                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    if (!DeductCoins(connection, transaction, userId.Value, userCoins))
                    {
                        transaction.Rollback();
                        Console.WriteLine($"An error occured while buying the package.");
                        return new Response(500, "Internal Server Error occured.");
                    }

                    if (!_packageManagement.DeletePackage(connection, transaction, randomPackageId.Value))
                    {
                        transaction.Rollback();
                        Console.WriteLine("An error occured while deleting the aquired package.");
                        return new Response(500, "Internal Server Error occured.");
                    }

                    if (!_cardManagement.AddCardsToStack(connection, transaction, userId.Value, cardIds))
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
                return new Response(500, "An internal server error occurred.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while aquiring package: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
        }

        public Response GetUserData(string token, string username)
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null || connection.State != ConnectionState.Open)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }

                string? tokenUsername = GetUsername(connection, token);
                if (tokenUsername == null)
                {
                    return new Response(500, "Internal Server Error occured.");
                }

                if (username != tokenUsername)
                {
                    Console.WriteLine($"{tokenUsername} is unauthorized to view user data of {username}.");
                    return new Response(401, "Unauthorized.");
                }

                using NpgsqlCommand command = new("SELECT * FROM users WHERE username = @username", connection);
                command.Parameters.AddWithValue("username", username);

                // Add query results to the user class
                using var reader = command.ExecuteReader();
                Response response;

                if (reader.Read())
                {
                    string queryUsername = reader.GetString(reader.GetOrdinal("username"));
                    string queryPassword = reader.GetString(reader.GetOrdinal("password"));

                    User user = new(queryUsername, queryPassword);

                    if (!reader.IsDBNull(reader.GetOrdinal("name")))
                    {
                        user.Name = reader.GetString(reader.GetOrdinal("name"));
                    }
                    else
                    {
                        user.Name = "";
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("bio")))
                    {
                        user.Bio = reader.GetString(reader.GetOrdinal("bio"));
                    }
                    else
                    {
                        user.Bio = "";
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("image")))
                    {
                        user.Image = reader.GetString(reader.GetOrdinal("image"));
                    }
                    else
                    {
                        user.Image = "";
                    }

                    response = new(200, user);
                    return response;
                }

                Console.WriteLine($"User {username} not found.");
                return new Response(404, "User not found.");
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while retrieving user data: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
        }

        public Response GetUserStats(string token)
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null || connection.State != ConnectionState.Open)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }

                Guid? userId = GetUserId(connection, token);
                if (userId == null)
                {
                    return new Response(500, "Internal Server Error occured.");
                }

                using NpgsqlCommand command = new("SELECT * FROM user_stats WHERE user_id = @userId", connection);
                command.Parameters.AddWithValue("userId", userId);

                // Add query results to the user class
                using var reader = command.ExecuteReader();
                Response response;

                if (reader.Read())
                {
                    int elo = reader.GetInt32(reader.GetOrdinal("elo"));
                    int coins = reader.GetInt32(reader.GetOrdinal("coins"));
                    int gamesPlayed = reader.GetInt32(reader.GetOrdinal("games_played"));
                    int wins = reader.GetInt32(reader.GetOrdinal("wins"));
                    int losses = reader.GetInt32(reader.GetOrdinal("losses"));

                    User user = new();
                    user.SetUserStats(elo, coins, gamesPlayed, wins, losses);

                    response = new(200, user.Stats);
                    return response;
                }

                Console.WriteLine($"User with id {userId} not found.");
                return new Response(404, "User not found.");
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while retrieving user stats: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
        }

        //  Missing Implementation
        public Response UpdateUserData(Dictionary<string, string> requestBody, string token, string username)
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null || connection.State != ConnectionState.Open)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }

                string? tokenUsername = GetUsername(connection, token);
                if (tokenUsername == null)
                {
                    return new Response(500, "Internal Server Error occured.");
                }

                if (username != tokenUsername)
                {
                    Console.WriteLine($"User with token {token} is unauthorized to view user data of {username}.");
                    return new Response(401, "Unauthorized.");
                }

                Console.WriteLine(requestBody);

                if (!requestBody.ContainsKey("Name") || !requestBody.ContainsKey("Bio") || !requestBody.ContainsKey("Image"))
                {
                    Console.WriteLine($"Data to change couldn't be found.");
                    return new Response(409, "Data to change couldn't be found.");
                }

                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    using NpgsqlCommand command = new("UPDATE users SET name = @newName, bio = @newBio, image = @newImage WHERE username = @username", connection);
                    command.Parameters.AddWithValue("newName", requestBody["Name"]);
                    command.Parameters.AddWithValue("newBio", requestBody["Bio"]);
                    command.Parameters.AddWithValue("newImage", requestBody["Image"]);
                    command.Parameters.AddWithValue("username", username);

                    if (command.ExecuteNonQuery() == 0)
                    {
                        transaction.Rollback();
                        Console.WriteLine("User data could not be updated.");
                        return new Response(500, "An internal server error occurred.");
                    }
                    transaction.Commit();
                }
                return new Response(200, "User data updated successfully.");
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while updating user data: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
        }

        public Response ConfigureUserDeck(List<string> cardIds, string token)
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null || connection.State != ConnectionState.Open)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }

                Guid? userId = GetUserId(connection, token);
                if (userId == null)
                {
                    return new Response(500, "Internal Server Error occured.");
                }

                //  Check if there are cards in the stack
                int stackSize = _cardManagement.GetStackSize(connection, userId.Value);
                if (stackSize < 0)
                {
                    return new Response(500, "Internal Server Error occured.");
                }
                if (stackSize == 0)
                {
                    return new Response(409, "No cards in stack to add to deck.");
                }

                //  Check if the cards to add are in the user stack
                if (!_cardManagement.CheckIfCardsInStack(connection, cardIds, userId.Value))
                {
                    return new Response(409, "Only cards in stack can be added to the deck.");
                }

                //  Check if the cards to add are already in the user deck
                if (_cardManagement.CheckIfCardInDeck(connection, cardIds, userId.Value))
                {
                    return new Response(409, "Card is already in deck.");
                }

                //  Check if the cards to add are 5 maximum and don't exceed the deck limit
                int cardCount = cardIds.Count;
                int deckSize = _cardManagement.GetDeckSize(connection, userId.Value);
                if (deckSize < 0)
                {
                    return new Response(500, "Internal Server Error occured.");
                }

                int maxDeckSize = _cardManagement.GetMaxDeckSize();
                int combinedCardCount = deckSize + cardCount;
                if (cardCount > maxDeckSize || combinedCardCount > maxDeckSize)
                {
                    Console.WriteLine($"Too many cards, deck can only hold 5 cards {cardCount}");
                    return new Response(409, $"Too many cards added: {cardCount}. Deck can only hold up to {maxDeckSize}");
                }

                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    //  Add the cards to the user deck
                    if (!_cardManagement.AddCardsToDeck(connection, transaction, cardIds, userId.Value))
                    {
                        transaction.Rollback();
                        return new Response(500, "Internal Server Error occured.");
                    }
                    transaction.Commit();
                }
                return new Response(200, "Deck updated successfully!");
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while configuring user deck: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
        }

        public bool DeductCoins(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid userId, int userCoins)
        {
            int newCoins = userCoins - _packageManagement.GetPackageCost();
            using NpgsqlCommand command = new("UPDATE user_stats SET coins = @newCoins WHERE user_id = @userId", connection, transaction);
            command.Parameters.AddWithValue("newCoins", newCoins);
            command.Parameters.AddWithValue("userId", userId);

            return command.ExecuteNonQuery() > 0;
        }

        public Response GetUserStack(string token)
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null || connection.State != ConnectionState.Open)
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
                if (stack == null || stack.Count <= 0)
                {
                    Console.WriteLine("Stack is empty");
                }

                return new Response(200, stack);
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while retrieving user stack: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
        }

        public Response GetUserDeck(string token)
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null || connection.State != ConnectionState.Open)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }

                Guid? userId = GetUserId(connection, token);
                if (userId == null)
                {
                    return new Response(500, "Internal Server Error occured.");
                }

                List<Card> deck = _cardManagement.GetDeck(connection, userId.Value);
                if (deck == null || deck.Count <= 0)
                {
                    Console.WriteLine("Deck is empty");
                }

                return new Response(200, deck);
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while retrieving user deck: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
        }

        public Response GetScoreboard()
        {
            try
            {
                using NpgsqlConnection connection = _dbConnection.OpenConnection();
                if (connection == null || connection.State != ConnectionState.Open)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }

                using NpgsqlCommand command = new("SELECT u.username, s.elo FROM users u JOIN user_stats s ON u.id = s.user_id ORDER BY s.elo DESC", connection);
                using var reader = command.ExecuteReader();
                List<ScoreBoard> scoreboard = new();

                while (reader.Read())
                {
                    if (reader.IsDBNull(0) || reader.IsDBNull(1))
                    {
                        return new Response(500, "Internal Server Error occured.");
                    }

                    ScoreBoard sb = new(reader.GetString(0), reader.GetInt32(1));
                    scoreboard.Add(sb);
                }

                return new Response(200, scoreboard);
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while retrieving scoreboard: {e.Message}");
                return new Response(500, "An internal server error occurred.");
            }
        }

        //  Helper method to check if a username is in the database
        public bool CheckIfUserExists(NpgsqlConnection connection, string username)
        {
            using NpgsqlCommand command = new("SELECT COUNT(*) FROM users WHERE username = @username", connection);
            command.Parameters.AddWithValue("username", username);

            //  Check if the username already exists, UNIQUE constraint is set so it should never be > 1
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        //  Helper method that checks if a user has admin priviledges
        public bool CheckIfAdmin(NpgsqlConnection connection, string token)
        {
            using NpgsqlCommand command = new("SELECT token FROM users WHERE username = @username", connection);
            command.Parameters.AddWithValue("@username", "admin");

            string? resultToken = command.ExecuteScalar()?.ToString();

            if (resultToken == null || resultToken != token)
            {
                return false;
            }

            return true;
        }

        public Guid? GetUserId(NpgsqlConnection connection, string token)
        {
            using NpgsqlCommand command = new("SELECT id FROM users WHERE token = @token", connection);
            command.Parameters.AddWithValue("token", token);
            Guid? userId = (Guid?)command.ExecuteScalar();

            return userId;
        }

        public string? GetUsername(NpgsqlConnection connection, string token)
        {
            using NpgsqlCommand command = new("SELECT username FROM users WHERE token = @token", connection);
            command.Parameters.AddWithValue("token", token);

            return command.ExecuteScalar()?.ToString();
        }

        public int GetUserCoins(NpgsqlConnection connection, Guid userId)
        {
            using NpgsqlCommand command = new("SELECT coins FROM user_stats WHERE user_id = @userId", connection);
            command.Parameters.AddWithValue("userId", userId);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        public bool CheckIfTokenIsValid(string token)
        {
            using NpgsqlConnection connection = _dbConnection.OpenConnection();
            return _tokenManagement.CheckIfTokenIsValid(connection, token);
        }
    }
}
