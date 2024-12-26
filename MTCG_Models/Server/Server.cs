using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using Npgsql;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MonsterTradingCardsGame.MTCG_Models.Database;
using MonsterTradingCardsGame.MTCG_Models.Models;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class Server
    {
        private readonly TcpListener _listener;
        private bool _isRunning = true;

        public Server(string url)
        {
            var uri = new Uri(url);
            int port = uri.Port;
            _listener = new TcpListener(IPAddress.Any, port);
        }

        //  This method is the starting point of the server
        public async Task StartServer()
        {
            try
            {
                _listener.Start();
                Console.WriteLine("Server started.");
                Console.WriteLine("Waiting for a connection...");

                //  Create Shop if it doesn't exist
                int shopId = 1;
                DatabaseConnection dbConnection = new();
                using NpgsqlConnection connection = dbConnection.OpenConnection();
                if (connection == null)
                {
                    Console.WriteLine($"Connection failed. Status: {connection.State}");
                    Console.WriteLine($"An internal server error occurred.");
                    StopServer();
                }

                if (!CheckIfShopExists(connection, shopId))
                {
                    CreateShop(connection, shopId);
                    Console.WriteLine("New shop created!");
                }
                else
                {
                    Console.WriteLine("Shop already exists.");
                }

                while (_isRunning)
                {
                    using TcpClient client = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine("\nNew client connected!");

                    _ = RequestHandler(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"SocketException: {e}");
                StopServer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                StopServer();
            }
            finally
            {
                StopServer();
            }
        }

        //  Method that handles Client Requests
        public async Task RequestHandler(TcpClient client)
        {
            try
            {
                // Get a network stream object for reading from and writing to the client
                NetworkStream stream = client.GetStream();
                using StreamWriter writer = new(stream);
                byte[] bytes = new byte[1024];
                int bytesRead;
                string requestStr;

                bytesRead = await stream.ReadAsync(bytes, 0, bytes.Length);
                // Translate data bytes to an ASCII string.
                requestStr = Encoding.UTF8.GetString(bytes, 0, bytesRead);

                if(bytesRead <= 0)
                {
                    Console.WriteLine("Request is empty, server is closing connection.");
                    return;
                }

                Console.WriteLine($"Received:\n {requestStr}");

                Router router = new(requestStr);
                var response = router.RequestHandler();

                await writer.WriteLineAsync(response.GetResponse());
                Console.WriteLine($"\nResponse:\n {response.GetResponse()}");

                if (response.CheckIfServerError())
                {
                    Console.WriteLine("Server error detected. Initiating shutdown...");
                    StopServer();
                }

                writer.Flush();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IOException: {ex.Message}");
            }
            catch (OutOfMemoryException ex)
            {
                Console.WriteLine($"OutOfMemoryException: {ex.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.WriteLine("Connection closed.");
                client.Close();
            }
        }

        public bool CheckIfShopExists(NpgsqlConnection connection, int id)
        {
            using NpgsqlCommand command = new("SELECT id FROM shop WHERE @id = id", connection);
            command.Parameters.AddWithValue("id", id);

            object resultObj = command.ExecuteScalar();

            if (resultObj == null)
            {
                return false;
            }

            return true;
        }

        public void CreateShop(NpgsqlConnection connection, int id)
        {
            using NpgsqlCommand command = new("INSERT INTO shop (id) VALUES (@id)", connection);
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void StopServer()
        {
            _isRunning = false;
            _listener.Stop();
            Console.WriteLine("Server initiating Shutdown...");
        }
    }
}