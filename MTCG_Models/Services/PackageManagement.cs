using MonsterTradingCardsGame.MTCG_Models.Database;
using MonsterTradingCardsGame.MTCG_Models.Models;
using MonsterTradingCardsGame.MTCG_Models.Server;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MonsterTradingCardsGame.MTCG_Models.Services
{
    public class PackageManagement
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly CardManagement _cardManagement;
        private const int _packageSize = 5;
        private const int _packageCost = 5;
        private readonly int _shopId = 1;

        public PackageManagement(DatabaseConnection dbConnection, int shopId)
        {
            _dbConnection = dbConnection;
            _cardManagement = new();
            _shopId = shopId;
        }

        public Response CreatePackage(List<Card> cards)
        {
            using NpgsqlConnection connection = _dbConnection.OpenConnection();
            using (NpgsqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    if (connection == null || connection.State != System.Data.ConnectionState.Open)
                    {
                        Console.WriteLine("Connection to Database failed.");
                        return new Response(500, "Internal Server Error occured.");
                    }
                    if (cards.Count != _packageSize)
                    {
                        return new Response(403, "A package can only contain 5 cards..");
                    }

                    //  Create a new package
                    using NpgsqlCommand command = new("INSERT INTO package (shop_id) VALUES (@shopId) RETURNING id", connection);
                    command.Parameters.AddWithValue("shopId", _shopId);

                    //  Retrieve the new package id
                    Guid? packageId = (Guid?)command.ExecuteScalar();
                    if (packageId == null)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Error occured while retrieving package ID.");
                        return new Response(500, "An internal server error occurred.");
                    }
                    Console.WriteLine($"Package with ID {packageId} has been added to the database.");

                    //  Add cards to the database
                    if (!_cardManagement.AddCardsToDatabase(connection, transaction, cards, packageId.Value))
                    {
                        transaction.Rollback();
                        return new Response(403, "Card already exists.");
                    }
                    transaction.Commit();

                    Console.WriteLine($"Cards have been added to the package.");
                    return new Response(201, "Package and cards successfully created.");
                }
                catch (NpgsqlException e)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Failed to connect to Database: {e.Message}");
                    return new Response(500, "An internal server error occurred.");
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Console.WriteLine($"An error occurred while creating package: {e.Message}");
                    return new Response(500, "An internal server error occurred.");
                }
            }
        }

        public Guid? GetRandomPackageId(NpgsqlConnection connection)
        {
            using NpgsqlCommand command = new("SELECT id FROM package WHERE shop_id = @shopId ORDER BY RANDOM() LIMIT 1", connection);
            command.Parameters.AddWithValue("shopId", _shopId);
            return (Guid?)command.ExecuteScalar();
        }

        public List<string> GetCardIds(NpgsqlConnection connection, Guid packageId)
        {
            List<string> cardIds = new();

            using (var command = new NpgsqlCommand("SELECT id FROM card WHERE package_id = @packageId", connection))
            {
                command.Parameters.AddWithValue("packageId", packageId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id;
                        if(!reader.IsDBNull(0))
                        {
                            id = reader.GetString(0);
                        }
                        else
                        {
                            id = "";
                        }
                        cardIds.Add(id);
                    }
                }
            }
            return cardIds;
        }

        public bool DeletePackage(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid packageId)
        {
            using (NpgsqlCommand deletePackageId = new NpgsqlCommand("UPDATE card SET package_id = NULL WHERE package_id = @packageId", connection, transaction))
            {
                deletePackageId.Parameters.AddWithValue("packageId", packageId);
                int rowsAffected = deletePackageId.ExecuteNonQuery();
                if (rowsAffected <= 0)
                {
                    return false;
                }
            }

            using (NpgsqlCommand deletePackage = new NpgsqlCommand("DELETE FROM package WHERE id = @packageId", connection, transaction))
            {
                deletePackage.Parameters.AddWithValue("packageId", packageId);
                int rowsAffected = deletePackage.ExecuteNonQuery();
                if (rowsAffected <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetPackageCost()
        {
            return _packageCost;
        }

        public int GetPackageSize()
        {
            return _packageSize;
        }

    }
}
