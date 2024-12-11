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
    public class Server
    {
        private TcpListener _listener;

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

                while (true)
                {
                    using TcpClient client = _listener.AcceptTcpClient();
                    Console.WriteLine("\nNew client connected!");

                    RequestHandler(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"SocketException: {e}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _listener.Stop();
            }
        }

        //  Method that handles Client Requests
        public static void RequestHandler(TcpClient client)
        {
            // Get a network stream object for reading from and writing to the client
            using NetworkStream stream = client.GetStream();
            StreamWriter writer = new(stream);
            byte[] bytes = new byte[1024];
            string request;

            try
            {
                int bytesRead;
                while ((bytesRead = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to an ASCII string.
                    request = Encoding.UTF8.GetString(bytes, 0, bytesRead);
                    Console.WriteLine($"Received:\n {request}");

                    Dictionary<string, string> requestData = new();
                    requestData = Parser.ParseRequest(writer, request);
                    if(requestData == null)
                    {
                        HTTPResponse.Response(writer, 400, "Malformed Request Syntax.");
                        continue;
                    }

                    Routing.Router(writer, requestData);

                    writer.Flush();
                }
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