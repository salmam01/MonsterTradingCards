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
        private TcpListener _listener;
        public bool IsRunning { get; set; } = true;
        public bool _clientConnection = true;

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
                Console.WriteLine("Waiting for a connection...");

                while (IsRunning)
                {
                    using TcpClient client = _listener.AcceptTcpClient();
                    Console.WriteLine("\nNew client connected!");

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
            using StreamReader reader = new StreamReader(stream);

            //byte[] bytes = new byte[1024];
            string request;

            try
            {
                int bytesRead;
                while (_clientConnection)
                {
                    if(!client.Connected)
                    {
                        _clientConnection = false;
                        Console.WriteLine("Client has closed the connection.");
                    }

                    // Translate data bytes to an ASCII string.
                    //request = Encoding.UTF8.GetString(bytes, 0, bytesRead);
                    if(!stream.DataAvailable)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    request = reader.ReadLine();
                    if (!Parser.CheckIfValidString(request))
                    {
                        Console.WriteLine("Client sent an empty request.");
                        reader.Close();
                        break;
                    }

                    Console.WriteLine($"Received:\n {request}");

                    Router router = new(request);
                    var response = router.HandleRequest();

                    writer.Write(response);
                    writer.Flush();
                    
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IOException: {ex.Message}");
                IsRunning = false;
            }
            catch (OutOfMemoryException ex)
            {
                Console.WriteLine($"OutOfMemoryException: {ex.Message}");
                IsRunning = false;
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

    }
}