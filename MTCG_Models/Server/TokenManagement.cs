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
        public static string GenerateToken(string username)
        {
            return username + "-mtcgToken";
        }

        public static bool CheckIfTokenIsValid(string username, string token)
        {
            try
            {
                DatabaseConnection dbConnection = new DatabaseConnection();
                using NpgsqlConnection connection = dbConnection.OpenConnection();
                if (connection == null)
                {
                    Console.WriteLine("Connection failed.");
                    return false;
                }

                //  Still don't understand prepared statements lol
                using NpgsqlCommand command = new("SELECT @username FROM player WHERE token = @token", connection);
                command.Parameters.AddWithValue("username", username);
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

        public static void SendTokenToClient(StreamWriter writer, int statusCode, string message, string token)
        {
            string response = JsonSerializer.Serialize(new
            {
                message,
                token
            });

            writer.WriteLine($"HTTP/1.1 {statusCode}");
            writer.WriteLine("Content-Type: application/json");
            writer.WriteLine();
            writer.WriteLine(response);
        }
    }
}
