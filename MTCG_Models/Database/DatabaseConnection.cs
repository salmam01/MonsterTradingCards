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
        private readonly string _host;
        private readonly string _port;
        private readonly string _username;    
        private readonly string _password;
        private readonly string _database;

        public DatabaseConnection()
        {
            _host = "localhost";
            _port = "5432";
            _username = "salma";
            _password = "mtcg1234";
            _database = "mtcg_database";
        }

        public NpgsqlConnection OpenConnection()
        {

            string connectionString = $"Host={_host};Port={_port};Username={_username};Password={_password};Database={_database}";

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
