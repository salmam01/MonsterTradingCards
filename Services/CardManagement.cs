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
using System.Collections;

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

        public bool CheckIfCardsInDeck(NpgsqlConnection connection, List<string> cardIds, Guid userId)
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

        public UserStack GetStack(NpgsqlConnection connection, Guid userId)
        {
            using NpgsqlCommand command = new("SELECT c.id, c.name, c.damage FROM stack s INNER JOIN card c ON s.card_id = c.id WHERE s.user_id = @userId", connection);
            command.Parameters.AddWithValue("userId", userId);
            using var reader = command.ExecuteReader();
            List<Card> cards = new();

            while (reader.Read())
            {
                string id;
                string name;
                double damage;

                if (!reader.IsDBNull(reader.GetOrdinal("id")))
                {
                    id = reader.GetString(reader.GetOrdinal("id"));
                }
                else
                {
                    id = "";
                }
                if (!reader.IsDBNull(reader.GetOrdinal("name")))
                {
                    name = reader.GetString(reader.GetOrdinal("name"));
                }
                else
                {
                    name = "";
                }
                if (!reader.IsDBNull(reader.GetOrdinal("damage")))
                {
                    damage = reader.GetDouble(reader.GetOrdinal("damage"));
                }
                else
                {
                    damage = 0;
                }

                Card card = new(id, name, damage);
                cards.Add(card);
            }

            return new UserStack(cards);
        }

        public UserDeck GetDeck(NpgsqlConnection connection, Guid userId)
        {
            using NpgsqlCommand command = new("SELECT c.id, c.name, c.damage FROM deck d INNER JOIN card c ON d.card_id = c.id WHERE d.user_id = @userId", connection);
            command.Parameters.AddWithValue("userId", userId);
            using var reader = command.ExecuteReader();
            List<Card> cards = new();

            while (reader.Read())
            {
                string id;
                string name;
                double damage;

                if (!reader.IsDBNull(reader.GetOrdinal("id")))
                {
                    id = reader.GetString(reader.GetOrdinal("id"));
                }
                else
                {
                    id = "";
                }
                if (!reader.IsDBNull(reader.GetOrdinal("name")))
                {
                    name = reader.GetString(reader.GetOrdinal("name"));
                }
                else
                {
                    name = "";
                }
                if (!reader.IsDBNull(reader.GetOrdinal("damage")))
                {
                    damage = reader.GetDouble(reader.GetOrdinal("damage"));
                }
                else
                {
                    damage = 0;
                }

                Card card = new(id, name, damage);
                cards.Add(card);
            }
            return new UserDeck(cards);
        }

        public bool UpdateStack(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid userId, UserDeck deck)
        {
            if(deck.DeletedCards != null && deck.DeletedCards.Count > 0)
            {
                foreach(Card deletedCard in deck.DeletedCards)
                {
                    using NpgsqlCommand command = new("DELETE FROM stack WHERE user_id = @userId AND card_id = @cardId", connection, transaction);
                    command.Parameters.AddWithValue("userId", userId);
                    command.Parameters.AddWithValue("cardId", deletedCard.Id);

                    if(command.ExecuteNonQuery() == 0)
                    {
                        return false;
                    }
                }
            }

            if (deck.AddedCards != null && deck.AddedCards.Count > 0)
            {
                foreach (Card addedCard in deck.AddedCards)
                {
                    using NpgsqlCommand command = new("INSERT INTO stack (user_id, card_id) VALUES (@userId, @cardId)", connection, transaction);
                    command.Parameters.AddWithValue("userId", userId);
                    command.Parameters.AddWithValue("cardId", addedCard.Id);

                    if (command.ExecuteNonQuery() == 0)
                    {
                        Console.WriteLine("Error adding card to stack.");
                        return false;
                    }
                }
            }
            return true;
        }

        public bool UpdateDeck(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid userId, UserDeck deck)
        {
            if(deck.AddedCards != null && deck.AddedCards.Count > 0)
            {
                foreach (Card card in deck.AddedCards)
                {
                    if (!CheckIfCardExists(connection, card.Id))
                    {
                        return false;
                    }

                    using NpgsqlCommand command = new("INSERT INTO deck (user_id, card_id) VALUES (@userId, @cardId)", connection, transaction);
                    command.Parameters.AddWithValue("userId", userId);
                    command.Parameters.AddWithValue("cardId", card.Id);

                    if (command.ExecuteNonQuery() == 0)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        public bool DeleteDeck(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid userId)
        {
            using NpgsqlCommand command = new("DELETE FROM deck WHERE user_id = @userId", connection, transaction);
            command.Parameters.AddWithValue("userId", userId);
            return command.ExecuteNonQuery() > 0;
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
