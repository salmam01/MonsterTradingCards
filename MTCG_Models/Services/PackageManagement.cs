using MonsterTradingCardsGame.MTCG_Models.Models;
using MonsterTradingCardsGame.MTCG_Models.Server;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Services
{
    public class PackageManagement
    {
        private readonly NpgsqlConnection _connection;
        private readonly CardManagement _cardManagement;
        private const int _packageSize = 5;
        private const int _packageCost = 5;
        private readonly int _shopId = 1;

        public PackageManagement(NpgsqlConnection connection, int shopId)
        {
            _connection = connection;
            _cardManagement = new(_connection);
            _shopId = shopId;
        }

        public Response CreatePackage(List<Card> cards)
        {
            try
            {
                if (_connection == null)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }
                if (cards.Count != _packageSize)
                {
                    return new Response(403, "A package can only contain 5 cards..");
                }

                //  Create a new package
                using NpgsqlCommand command = new("INSERT INTO package (shop_id) VALUES (@shopId) RETURNING id", _connection);
                command.Parameters.AddWithValue("shopId", _shopId);

                //  Retrieve the new package id
                string? packageId = command.ExecuteScalar()?.ToString();
                if (packageId == null)
                {
                    Console.WriteLine("Error occured while retrieving package ID.");
                    return new Response(500, "An internal server error occurred.");
                }
                Console.WriteLine($"Package with ID {packageId} has been added to the database.");

                //  Add cards to the database
                if (!_cardManagement.AddCardsToDatabase(cards, packageId))
                {
                    return new Response(500, "An internal server error occurred.");
                }

                Console.WriteLine($"Cards have been added to the package.");
                return new Response(201, "Package and cards successfully created.");
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "Internal Server Error occured.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during login: {e.Message}");
                return new Response(500, "Internal Server Error occured.");
            }
        }

        public Response AquirePackage(int coins)
        {
            try
            {
                if (_connection == null)
                {
                    Console.WriteLine("Connection to Database failed.");
                    return new Response(500, "Internal Server Error occured.");
                }
                if (coins < _packageCost)
                {
                    Console.WriteLine($"Not enough money for buying a card package. Coins: {coins}");
                    return new Response(403, "Not enough money for buying a card package.");
                }

                //  Select random package if any exists
                Guid? randomPackageId = GetRandomPackageId();
                if (randomPackageId == null)
                {
                    Console.WriteLine($"No packages available for purchase.");
                    return new Response(404, "No card package available for buying.");
                }

                List<string> cardIds = new();

                // Make sure the SELECT query completes before moving forward
                using NpgsqlCommand command = new("SELECT id FROM card WHERE package_id = @packageId", _connection);
                command.Parameters.AddWithValue("packageId", randomPackageId);

                using NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    cardIds.Add(reader["id"].ToString());
                }

                if (cardIds.Count < 5)
                {
                    Console.WriteLine($"Incorrect number of cards in package: {cardIds.Count}");
                    return new Response(500, "Internal Server Error occured.");
                }

                //  Delete the package once done
                if (!DeletePackage(randomPackageId.Value))
                {
                    Console.WriteLine("An error occured while deleting the aquired package.");
                    return new Response(500, "Internal Server Error occured.");
                }

                Console.WriteLine("Packages aquired successfully.");
                return new Response(200, "A package has been successfully bought.");
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return new Response(500, "Internal Server Error occured.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during login: {e.Message}");
                return new Response(500, "Internal Server Error occured.");
            }
        }

        public bool DeletePackage(Guid packageId)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    using NpgsqlCommand deletePackageId = new("UPDATE card SET package_id = NULL WHERE package_id = @packageId", _connection, transaction);
                    deletePackageId.Parameters.AddWithValue("packageId", packageId);
                    deletePackageId.ExecuteNonQuery();

                    using NpgsqlCommand deletePackage = new("DELETE FROM package WHERE id = @packageId", _connection, transaction);
                    deletePackage.Parameters.AddWithValue("packageId", packageId);
                    deletePackage.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error while deleting package: {e.Message}");
                    return false;
                }
            }
        }

        public Guid? GetRandomPackageId()
        {
            try
            {
                using NpgsqlCommand command = new("SELECT id FROM package WHERE shop_id = @shopId ORDER BY RANDOM() LIMIT 1", _connection);
                command.Parameters.AddWithValue("shopId", _shopId);
                string? randomId = command.ExecuteScalar()?.ToString();

                if (randomId == null)
                {
                    return null;
                }

                // Convert user ID to a UUID
                if (!Guid.TryParse(randomId, out Guid randomGuid))
                {
                    Console.WriteLine($"Invalid player ID: {randomId}");
                    return null;
                }

                return randomGuid;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during package counting: {e.Message}");
                return null;
            }
        }

    }
}
