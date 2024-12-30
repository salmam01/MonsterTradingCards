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
        private const int _maxDeckSize = 5;

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
            try
            {
                for(int i = 0; i < cardIds.Count; i++)
                {
                    using NpgsqlCommand command = new("INSERT INTO stack (player_id, card_id) VALUES (@userId, @cardId)", connection, transaction);
                    command.Parameters.AddWithValue("userId", userId);
                    command.Parameters.AddWithValue("cardId", cardIds[i]);
                    command.ExecuteNonQuery();
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
                Console.WriteLine($"Error occured while adding cards to stack: {e.Message}");
                return false;
            }
        }

        public bool AddCardsToDeck(NpgsqlConnection connection, List<string> cardIds, Guid userId)
        {
            try
            {
                for (int i = 0; i < cardIds.Count; i++)
                {
                    using NpgsqlCommand command = new("INSERT INTO deck (player_id, card_id) VALUES (@userId, @cardId)", connection);
                    command.Parameters.AddWithValue("userId", userId);
                    command.Parameters.AddWithValue("cardId", cardIds[i]);
                    command.ExecuteNonQuery();
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
                Console.WriteLine($"Error occured while adding cards to deck: {e.Message}");
                return false;
            }
        }
            
        public bool CheckIfCardsInStack(NpgsqlConnection connection, List<string> cardIds, Guid userId)
        {
            try
            {
                for (int i = 0; i < cardIds.Count; i++)
                {
                    using NpgsqlCommand command = new("SELECT 1 FROM stack WHERE player_id = @userId AND card_id = @cardId LIMIT 1", connection);
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
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured while adding cards to deck: {e.Message}");
                return false;
            }
        }

        public bool CheckIfCardIsAlreadyInDeck(NpgsqlConnection connection, List<string> cardIds, Guid userId)
        {
            try
            {
                for (int i = 0; i < cardIds.Count; i++)
                {
                    using NpgsqlCommand command = new("SELECT 1 FROM deck WHERE player_id = @userId AND card_id = @cardId LIMIT 1", connection);
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
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured while adding cards to deck: {e.Message}");
                return false;
            }
        }

        public List<Card> GetStack(NpgsqlConnection connection, Guid userId)
        {
            List<Card> stack = new();
            try
            {
                using NpgsqlCommand command = new("SELECT c.id, c.name, c.damage FROM stack s INNER JOIN card c ON s.card_id = c.id WHERE s.player_id = @userId", connection);
                command.Parameters.AddWithValue("userId", userId);
                using var reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    string id = reader["id"].ToString();
                    string name = reader["name"].ToString();
                    double damage = Convert.ToDouble(reader["damage"]);

                    Card card = new(id, name, damage);
                    stack.Add(card);
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured while retrieving player deck: {e.Message}");
            }
            return stack;
        }

        public int GetStackSize(NpgsqlConnection connection, Guid userId)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT COUNT(*) FROM stack WHERE player_id = @userId", connection);
                command.Parameters.AddWithValue("userId", userId);
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return -1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured while retrieving player deck size: {e.Message}");
                return -1;
            }
        }

        public List<Card> GetDeck(NpgsqlConnection connection, Guid userId)
        {
            List<Card> deck = new();
            try
            {
                using NpgsqlCommand command = new("SELECT c.id, c.name, c.damage FROM deck d INNER JOIN card c ON d.card_id = c.id WHERE d.player_id = @userId", connection);
                command.Parameters.AddWithValue("userId", userId);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string id = reader["id"].ToString();
                    string name = reader["name"].ToString();
                    double damage = Convert.ToDouble(reader["damage"]);

                    Card card = new(id, name, damage);
                    deck.Add(card);
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured while retrieving player deck: {e.Message}");
            }
            return deck;
        }

        public int GetDeckSize(NpgsqlConnection connection, Guid userId)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT COUNT(*) FROM deck WHERE player_id = @userId", connection);
                command.Parameters.AddWithValue("userId", userId);
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return -1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured while retrieving player deck size: {e.Message}");
                return -1;
            }
        }

        public bool CheckIfCardExists(NpgsqlConnection connection, string cardId)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT COUNT(*) FROM card WHERE id = @cardId", connection);
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

        public int GetMaxDeckSize()
        {
            return _maxDeckSize;
        }
    }
}
