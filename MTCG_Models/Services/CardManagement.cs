using MonsterTradingCardsGame.MTCG_Models.Database;
using MonsterTradingCardsGame.MTCG_Models.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MonsterTradingCardsGame.MTCG_Models.Services
{
    public class CardManagement
    {
        private readonly DatabaseConnection _dbConnection;
        private const int _maxDeckSize = 4;

        public CardManagement(DatabaseConnection dbConnection) 
        {
            _dbConnection = dbConnection;
        }

        public bool AddCardsToDatabase(List<Card> cards, Guid packageId)
        {
            using NpgsqlConnection connection = _dbConnection.OpenConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var card in cards)
                {
                    if (CheckIfCardExists(connection, card.Id))
                    {
                        Console.WriteLine($"Card {card.Name} with ID {card.Id} already exists, aborting transaction.");
                        transaction.Rollback();
                        return false;
                    }

                    using NpgsqlCommand command = new("INSERT INTO card (id, name, damage, package_id) VALUES (@id, @name, @damage, @packageId)", connection);
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

        public bool AddCardsToStack(Guid userId, List<string> cardIds, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            for(int i = 0; i < cardIds.Count; i++)
            {
                using NpgsqlCommand command = new("INSERT INTO stack (user_id, card_id) VALUES (@userId, @cardId)", connection, transaction);
                command.Parameters.AddWithValue("userId", userId);
                command.Parameters.AddWithValue("cardId", cardIds[i]);
                int rowsAffected = command.ExecuteNonQuery();
                if(rowsAffected <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool AddCardsToDeck(NpgsqlConnection connection, List<string> cardIds, Guid userId)
        {
            for (int i = 0; i < cardIds.Count; i++)
            {
                using NpgsqlCommand command = new("INSERT INTO deck (user_id, card_id) VALUES (@userId, @cardId)", connection);
                command.Parameters.AddWithValue("userId", userId);
                command.Parameters.AddWithValue("cardId", cardIds[i]);
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected <= 0)
                {
                    return false;
                }
            }
            return true;
        }
            
        public bool CheckIfCardsInStack(NpgsqlConnection connection, List<string> cardIds, Guid userId)
        {
            for (int i = 0; i < cardIds.Count; i++)
            {
                using NpgsqlCommand command = new("SELECT 1 FROM stack WHERE user_id = @userId AND card_id = @cardId LIMIT 1", connection);
                command.Parameters.AddWithValue("userId", userId);
                command.Parameters.AddWithValue("cardId", cardIds[i]);

                var result = command.ExecuteScalar();

                if (result == null)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CheckIfCardIsAlreadyInDeck(NpgsqlConnection connection, List<string> cardIds, Guid userId)
        {
            for (int i = 0; i < cardIds.Count; i++)
            {
                using NpgsqlCommand command = new("SELECT 1 FROM deck WHERE user_id = @userId AND card_id = @cardId LIMIT 1", connection);
                command.Parameters.AddWithValue("userId", userId);
                command.Parameters.AddWithValue("cardId", cardIds[i]);

                var result = command.ExecuteScalar();

                if (result != null)
                {
                    return true;
                }
            }
            return false;
        }

        public List<Card> GetStack(NpgsqlConnection connection, Guid userId)
        {
            List<Card> stack = new();
            using NpgsqlCommand command = new("SELECT c.id, c.name, c.damage FROM stack s INNER JOIN card c ON s.card_id = c.id WHERE s.user_id = @userId", connection);
            command.Parameters.AddWithValue("userId", userId);
            using var reader = command.ExecuteReader();
                
            while (reader.Read())
            {
                string id;
                string name;
                double damage;

                if (!reader.IsDBNull(reader.GetOrdinal("id")))
                {
                    id = reader.GetOrdinal("id").ToString();
                }
                else
                {
                    id = "";
                }
                if (!reader.IsDBNull(reader.GetOrdinal("name")))
                {
                    name = reader.GetOrdinal("name").ToString();
                }
                else
                {
                    name = "";
                }
                if (!reader.IsDBNull(reader.GetOrdinal("damage")))
                {
                    damage = Convert.ToDouble(reader.GetOrdinal("damage"));
                }
                else
                {
                    damage = 0;
                }

                Card card = new(id, name, damage);
                stack.Add(card);
            }
            return stack;
        }

        public List<Card> GetDeck(NpgsqlConnection connection, Guid userId)
        {
            List<Card> deck = new();

            using NpgsqlCommand command = new("SELECT c.id, c.name, c.damage FROM deck d INNER JOIN card c ON d.card_id = c.id WHERE d.user_id = @userId", connection);
            command.Parameters.AddWithValue("userId", userId);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                string id;
                string name;
                double damage;

                if (!reader.IsDBNull(reader.GetOrdinal("id")))
                {
                    id = reader.GetOrdinal("id").ToString();
                }
                else
                {
                    id = "";
                }
                if (!reader.IsDBNull(reader.GetOrdinal("name")))
                {
                    name = reader.GetOrdinal("name").ToString();
                }
                else
                {
                    name = "";
                }
                if (!reader.IsDBNull(reader.GetOrdinal("damage")))
                {
                    damage = Convert.ToDouble(reader.GetOrdinal("damage"));
                }
                else
                {
                    damage = 0;
                }

                Card card = new(id, name, damage);
                deck.Add(card);
            }

            return deck;
        }

        public int GetStackSize(NpgsqlConnection connection, Guid userId)
        {
            using NpgsqlCommand command = new("SELECT COUNT(*) FROM stack WHERE user_id = @userId", connection);
            command.Parameters.AddWithValue("userId", userId);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public int GetDeckSize(NpgsqlConnection connection, Guid userId)
        {
            using NpgsqlCommand command = new("SELECT COUNT(*) FROM deck WHERE user_id = @userId", connection);
            command.Parameters.AddWithValue("userId", userId);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public bool CheckIfCardExists(NpgsqlConnection connection, string cardId)
        {
            using NpgsqlCommand command = new("SELECT COUNT(*) FROM card WHERE id = @cardId", connection);
            command.Parameters.AddWithValue("cardId", cardId);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public int GetMaxDeckSize()
        {
            return _maxDeckSize;
        }
    }
}
