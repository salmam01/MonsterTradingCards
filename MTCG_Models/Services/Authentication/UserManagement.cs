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
        private readonly TokenManagement _token;
        private readonly NpgsqlConnection _connection;
        private Guid _userId;

        public UserManagement(NpgsqlConnection connection)
        {
            _token = new();
            _connection = connection;
        }

        //  Method that handles registering a new user
        public Response SignUp(Dictionary<string, string> requestBody)
        {
            using var transaction = _connection.BeginTransaction();
            try
            {
                string username = requestBody["Username"];
                string password = requestBody["Password"];

                if (!Parser.CheckIfValidString(username) || !Parser.CheckIfValidString(password))
                {
                    Console.WriteLine("Username or Password is empty.");
                    return new Response(400, "Username or Password is empty.");
                }

                if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine($"Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }
                if (CheckIfUserExists(username))
                {
                    Console.WriteLine("User already exists.");
                    return new Response(409, "User already exists.");
                }
                Console.WriteLine("Connection successfully established with Database.");

                using NpgsqlCommand command = new("INSERT INTO player (username, password) VALUES (@username, @password) RETURNING id", _connection);

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
                if (!AddPlayerStats())
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
        public bool CheckIfUserExists(string username)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT COUNT(*) FROM player WHERE username = @username", _connection);
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

        public bool AddPlayerStats()
        {
            try
            {
                using NpgsqlCommand command = new("INSERT INTO player_stats (player_id) VALUES (@userId)", _connection);
                command.Parameters.AddWithValue("userId", _userId);
                command.ExecuteNonQuery();

                Console.WriteLine($"Player_stats has been added to UserID {_userId}.");
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
                if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }
                if (!CheckIfUserExists(username))
                {
                    Console.WriteLine("User doesn't exist.");
                    return new Response(401, "Incorrect Username or Password.");
                }

                using NpgsqlCommand command = new("SELECT password FROM player WHERE username = @username", _connection);
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
                    string token = _token.GenerateToken(_connection, username);

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

        //  Missing Implementation
        public Response GetUserData(Dictionary<string, string> requestBody, string token, string username)
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
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
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
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
        public bool CheckIfAdmin(string token)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT token FROM player WHERE username = @username", _connection);
                command.Parameters.AddWithValue("@username", "admin");

                object resultObj = command.ExecuteScalar();

                if (resultObj == null || resultObj.ToString() != token)
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

        public Guid? GetUserId(string token)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT id FROM player WHERE token = @token", _connection);
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

        public string? GetUsername(string token)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT username FROM player WHERE token = @token", _connection);
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

        public int GetUserCoins()
        {
            try
            {
                using NpgsqlCommand command = new("SELECT coins FROM player_stats WHERE player_id = @userId", _connection);
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

        public bool CheckIfTokenIsValid(string token)
        {
            return _token.CheckIfTokenIsValid(_connection, token);
        }
    }
}
