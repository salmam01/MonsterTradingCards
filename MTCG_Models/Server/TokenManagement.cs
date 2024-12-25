using MonsterTradingCardsGame.MTCG_Models.Database;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class TokenManagement
    {
        public string GenerateToken(string username)
        {
            string token = username + "-mtcgToken";

            if (AddTokenToDatabase(username, token))
            {
                Console.WriteLine($"{token} has been added to database!");
                return token;
            }
            else
            {
                Console.WriteLine("No user found with that username.");
                return "";
            }
        }

        public bool AddTokenToDatabase(string username, string token)
        {
            DatabaseConnection dbConnection = new();
            using NpgsqlConnection connection = dbConnection.OpenConnection();
            if (connection == null)
            {
                Console.WriteLine("Connection failed.");
                return false;
            }

            using NpgsqlCommand command = new("UPDATE player SET token = @token WHERE username = @username", connection);

            command.Parameters.AddWithValue("username", username);
            command.Parameters.AddWithValue("token", token);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckIfTokenIsValid(string token)
        {
            try
            {
                DatabaseConnection dbConnection = new();
                using NpgsqlConnection connection = dbConnection.OpenConnection();
                if (connection == null)
                {
                    Console.WriteLine("Connection failed.");
                    return false;
                }

                using NpgsqlCommand command = new("SELECT username FROM player WHERE token = @token", connection);
                command.Parameters.AddWithValue("token", token);
                object resultObj = command.ExecuteScalar();

                if (resultObj == null)
                {
                    return false;
                }

                return true;
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
    }
}
