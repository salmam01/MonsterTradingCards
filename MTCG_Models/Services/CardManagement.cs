using MonsterTradingCardsGame.MTCG_Models.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Services
{
    public class CardManagement
    {
        private readonly NpgsqlConnection _connection;

        public CardManagement(NpgsqlConnection connection) 
        {
            _connection = connection;
        }

        public bool AddCardsToDatabase(List<Card> cards, string packageId)
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
                    using NpgsqlCommand command = new("INSERT INTO card (id, name, damage, package_id) VALUES (@id, @name, @damage, @packageId)", _connection);
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
    }
}
