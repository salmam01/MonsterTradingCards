using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Database
{
    public class DatabaseConnection
    {
        public static NpgsqlConnection ConnectToDatabase()
        {
            string host = "localhost";
            string port = "5432";
            string username = "salma";
            string password = "mtcg1234";
            string database = "mtcg_database";

            string connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database}";

            try
            {
                NpgsqlConnection connection = new(connectionString);
                connection.Open();
                return connection;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                throw;
            }
        }
    }
}
