using MonsterTradingCardsGame.MTCG_Models.Database;
using MonsterTradingCardsGame.MTCG_Models.Server;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Services.Authentication
{
    public class TokenManagement
    {
        public string GenerateToken(NpgsqlConnection connection, string username)
        {
            string token = username + "-mtcgToken";

            if (AddTokenToDatabase(connection, username, token))
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

        public bool AddTokenToDatabase(NpgsqlConnection connection, string username, string token)
        {
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

        public bool CheckIfTokenIsValid(NpgsqlConnection connection, string token)
        {
            try
            {
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
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while checking for user: {e.Message}");
                return false;
            }
        }
    }
}
