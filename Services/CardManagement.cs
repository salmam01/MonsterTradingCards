using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Database;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MonsterTradingCardsGame.Services
{
    public class CardManagement
    {
        private const int _maxDeckSize = 4;

        public bool AddCardsToDatabase(NpgsqlConnection connection, NpgsqlTransaction transaction, List<Card> cards, Guid packageId)
        {
            foreach (var card in cards)
            {
                if (CheckIfCardExists(connection, card.Id))
                {
                    Console.WriteLine($"Card {card.Name} with ID {card.Id} already exists, aborting transaction.");
                    return false;
                }

                using NpgsqlCommand command = new("INSERT INTO card (id, name, damage, package_id) VALUES (@id, @name, @damage, @packageId)", connection, transaction);
                command.Parameters.AddWithValue("id", card.Id);
                command.Parameters.AddWithValue("name", card.Name);
                command.Parameters.AddWithValue("damage", card.Damage);
                command.Parameters.AddWithValue("packageId", packageId);

                if (command.ExecuteNonQuery() == 0)
                {
                    Console.WriteLine($"Error occurred while adding card {card.Name} to database.");
                    return false;
                }

                Console.WriteLine($"{card.Name} has been added to database!");
            }
            return true;
        }

        public bool AddCardsToStack(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid userId, List<string> cardIds)
        {
            for (int i = 0; i < cardIds.Count; i++)
            {
                using NpgsqlCommand command = new("INSERT INTO stack (user_id, card_id) VALUES (@userId, @cardId)", connection, transaction);
                command.Parameters.AddWithValue("userId", userId);
                command.Parameters.AddWithValue("cardId", cardIds[i]);

                if (command.ExecuteNonQuery() == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool AddCardsToDeck(NpgsqlConnection connection, NpgsqlTransaction transaction, List<string> cardIds, Guid userId)
        {
            for (int i = 0; i < cardIds.Count; i++)
            {
                using NpgsqlCommand command = new("INSERT INTO deck (user_id, card_id) VALUES (@userId, @cardId)", connection, transaction);
                command.Parameters.AddWithValue("userId", userId);
                command.Parameters.AddWithValue("cardId", cardIds[i]);

                if (command.ExecuteNonQuery() == 0)
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

        public bool CheckIfCardInDeck(NpgsqlConnection connection, List<string> cardIds, Guid userId)
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
