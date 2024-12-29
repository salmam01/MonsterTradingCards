using MonsterTradingCardsGame.MTCG_Models.Database;
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

        public bool AddCardsToDatabase(List<Card> cards, Guid packageId)
        {
            using var transaction = _connection.BeginTransaction();
            try
            {
                foreach (var card in cards)
                {
                    if (CheckIfCardExists(card.Id))
                    {
                        Console.WriteLine($"Card {card.Name} with ID {card.Id} already exists, aborting transaction.");
                        transaction.Rollback();
                        return false;
                    }

                    using NpgsqlCommand command = new("INSERT INTO card (id, name, damage, package_id) VALUES (@id, @name, @damage, @packageId)", _connection);
                    command.Parameters.AddWithValue("id", card.Id);
                    command.Parameters.AddWithValue("name", card.Name);
                    command.Parameters.AddWithValue("damage", card.Damage);
                    command.Parameters.AddWithValue("packageId", packageId);
                    command.ExecuteNonQuery();

                    Console.WriteLine($"{card.Name} has been added to database!");
                }

                transaction.Commit();
                return true;
            }
            catch (NpgsqlException e)
            {
                transaction.Rollback();
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                Console.WriteLine($"Error occured during login: {e.Message}");
                return false;
            }
        }

        public bool CheckIfCardExists(string cardId)
        {
            try
            {
                NpgsqlCommand command = new("SELECT COUNT(*) FROM card WHERE id = @cardId", _connection);
                command.Parameters.AddWithValue("cardId", cardId);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
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
