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

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    //-------------------------------------------------------->MISSING MORE ERROR HANDLING FOR REQUEST PARSING<---------------------------------------------------------------    
    //  Missing concurrency implementation

    public class Server
    {
        private readonly TcpListener _listener;
        public bool IsRunning { get; set; } = true;

        public Server(string url)
        {
            var uri = new Uri(url);
            int port = uri.Port;
            _listener = new TcpListener(IPAddress.Any, port);
        }

        //  This method is the starting point of the server
        public void StartServer()
        {
            try
            {
                _listener.Start();
                Console.WriteLine("Server started.");
                Console.WriteLine("Waiting for a connection...");

                while (IsRunning)
                {
                    using TcpClient client = _listener.AcceptTcpClient();
                    Console.WriteLine("\nNew client connected!");
                    //Task.Run(() => HandleClient(client));
                    RequestHandler(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"SocketException: {e}");
                IsRunning = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                IsRunning = false;
            }
            finally
            {
                _listener.Stop();
            }
        }

        //  Method that handles Client Requests
        public void RequestHandler(TcpClient client)
        {
            // Get a network stream object for reading from and writing to the client
            using NetworkStream stream = client.GetStream();
            using StreamWriter writer = new(stream);
            //using StreamReader reader = new StreamReader(stream);

            try
            {
                int bytesRead;
                byte[] bytes = new byte[1024];
                string? request;
                while (client.Connected)
                {
                    while ((bytesRead = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to an ASCII string.
                        request = Encoding.UTF8.GetString(bytes, 0, bytesRead);
                        Console.WriteLine($"Received:\n {request}");

                        Router router = new(request);
                        var response = router.RequestHandler();
                        writer.WriteLine(response.GetResponse());
                        Console.WriteLine(response.GetResponse());

                        if (response.CheckIfServerError())
                        {
                            Console.WriteLine("Server error detected. Initiating shutdown...");
                            StopServer();
                            break;
                        }
                        writer.Flush();
                    }
                }
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

        public void StopServer()
        {
            IsRunning = false;
            _listener.Stop();
            Console.WriteLine("Server initiating Shutdown...");
        }
    }
}