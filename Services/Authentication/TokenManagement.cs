using MonsterTradingCardsGame.Database;
using MonsterTradingCardsGame.Server;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Services.Authentication
{
    public class TokenManagement
    {
        public string? CreateToken(NpgsqlConnection connection, NpgsqlTransaction transaction, string username)
        {
            string? token = GenerateToken(username);

            if (token == null)
            {
                Console.WriteLine($"Failed to generate token for {username}");
                return null;
            }

            if (!AddTokenToDatabase(connection, transaction, username, token))
            {
                Console.WriteLine($"No user {username} found.");
                return null;
            }

            Console.WriteLine($"{token} has been added to database!");
            return token;
        }

        public string? GenerateToken(string username)
        {
            return username + "-mtcgToken";
        }

        public bool AddTokenToDatabase(NpgsqlConnection connection, NpgsqlTransaction transaction, string username, string token)
        {
            using NpgsqlCommand command = new("UPDATE users SET token = @token WHERE username = @username", connection, transaction);

            command.Parameters.AddWithValue("username", username);
            command.Parameters.AddWithValue("token", token);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                return true;
            }
            return false;
        }

        public bool CheckIfTokenIsValid(NpgsqlConnection connection, string token)
        {
            using NpgsqlCommand command = new("SELECT username FROM users WHERE token = @token", connection);
            command.Parameters.AddWithValue("token", token);
            object resultObj = command.ExecuteScalar();

            if (resultObj == null)
            {
                return false;
            }

            return true;
        }
    }
}
