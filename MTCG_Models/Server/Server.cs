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

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    //-------------------------------------------------------->MISSING MORE ERROR HANDLING FOR REQUEST PARSING<---------------------------------------------------------------
    public class Server
    {
        private TcpListener _listener;
        //private List<Player> _players = new();
        //private Mutex _mutex;

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

                    //  Pass handling the client requests in a seperate thread
                    //Task.Run(() => RequestHandler(client));
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

            // Buffer for reading data
            byte[] bytes = new byte[1024];
            string request;

            try
            {
                int bytesRead;
                while ((bytesRead = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to an ASCII string.
                    request = Encoding.UTF8.GetString(bytes, 0, bytesRead);
                    Console.WriteLine($"Received:\n{request}");

                    Dictionary<string, string> requestData = new();
                    requestData = Parser.ParseRequest(writer, request);
                    if(requestData == null)
                    {
                        HTTPResponse.Response(writer, 400, "Malformed Request Syntax.");
                        continue;
                    }

                    Router(writer, requestData);

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

        //  Method that redirects users depending on the HTTP method
        public static void Router(StreamWriter writer, Dictionary<string, string> request)
        {
            string method = request["Method"];

            switch (method)
            {
                case "GET":
                    GetRequestHandler(writer, request);
                    break;

                case "POST":
                    PostRequestHandler(writer, request);
                    break;

                case "PUT":
                    //  Code will be implemented later
                    HTTPResponse.Response(writer, 405, "Method not supported");
                    break;

                case "DELETE":
                    //  Code will be implemented later
                    HTTPResponse.Response(writer, 405, "Method not supported");
                    break;

                default:
                    HTTPResponse.Response(writer, 405, "Method not supported");
                    break;
            }
        }

        //  Method that handles GET Requests
        public static void GetRequestHandler(StreamWriter writer, Dictionary<string, string> request)
        {
            string path = request["Path"];
            Console.WriteLine($"Handling GET Request for {path}...");

            switch (path)
            {
                case "/":
                    HTTPResponse.Response(writer, 200, "Welcome to Monster Trading Cards Game!");
                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    HTTPResponse.Response(writer, 404, "Invalid HTTP Request Path");
                    break;
            }
        }

        //  Method that handles POST Requests
        public static void PostRequestHandler(StreamWriter writer, Dictionary<string, string> request)
        {
            string path = request["Path"];
            Console.WriteLine($"Handling POST Request for {path}..."); 

            int statusCode;
            switch (path)
            {
                //API-Endpoint for sign up
                case "/users":

                    Console.WriteLine($"Redirecting to {path}.");
                    statusCode = Register(request);
                    Console.WriteLine(statusCode);

                    if (statusCode == 201)
                    {
                        HTTPResponse.Response(writer, statusCode, "User created successfully");
                    }
                    else if (statusCode == 400)
                    {
                        HTTPResponse.Response(writer, statusCode, "Username and password are required");
                    }
                    else if (statusCode == 500)
                    {
                        HTTPResponse.Response(writer, statusCode, "Server was unable to connect to database");
                    }
                    else
                    {
                        HTTPResponse.Response(writer, statusCode, "Username already exists");
                    }

                    break;

                //  API-Endpoint for login
                case "/sessions":

                    Console.WriteLine($"Redirecting to {path}.");
                    statusCode = Login(request);
                    Console.WriteLine(statusCode);

                    if (statusCode == 200)
                    {
                        HTTPResponse.Response(writer, statusCode, "Login successful");
                    }
                    else
                    {
                        HTTPResponse.Response(writer, statusCode, "Invalid username / password provided");
                    }

                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    Console.WriteLine("Path invalid.");
                    HTTPResponse.Response(writer, 404, "Invalid HTTP Request Path");
                    break;
            }
        }

        public static NpgsqlConnection ConnectToDatabase()
        {
            string host = "localhost";
            string port = "5432";
            string username = "salma";
            string password = "mtcg1234";
            string database = "mtcg_database";

            string connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database}";

            try
            {
                NpgsqlConnection connection = new(connectionString);
                connection.Open();
                return connection;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");

                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                throw;
            }
        }

        //  Method that handles registering a new user
        public static int Register(Dictionary<string, string> request)
        {
            try
            {
                string username = request["Username"];
                string password = request["Password"];
                string token = GenerateToken(username);
                Console.WriteLine($"Generated token: {token}");

                //  Can make this a method
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
                {
                    return 400;
                }

                using NpgsqlConnection connection = ConnectToDatabase();
                if (connection == null)
                {
                    Console.WriteLine("Connection is null.");
                    return 500;
                }
                if (CheckIfUserExists(connection, username))
                {
                    Console.WriteLine("User already exists.");
                    return 409;
                }
                Console.WriteLine("Connection successfully established with Database.");

                using NpgsqlCommand command = new("INSERT INTO player (token, username, password) VALUES (@token, @username, @password)", connection);

                command.Parameters.AddWithValue("token", token);
                command.Parameters.AddWithValue("username", username);
                command.Parameters.AddWithValue("password", password);
                command.ExecuteNonQuery();
                Console.WriteLine($"{username} has been added to database!");

                return 201;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred while trying to signup user: {e.Message}");
                return 500;
            }
        }

        public static bool CheckIfUserExists(NpgsqlConnection connection, string username)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT COUNT(*) FROM player WHERE username = @username", connection);
                command.Parameters.AddWithValue("username", username);

                //  Check if the username already exists, UNIQUE constraint is set so it should never be > 1
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while checking for user: {e.Message}");
                throw;
            }
        }

        public static string GenerateToken(string username)
        {
            return username + "-mtcgToken";
        }

        //  Method that handles login from already registered Users
        public static int Login(Dictionary<string, string> data)
        {
            string username = data["Username"];
            string password = data["Password"];
            // FOR NOW, CHANGE LATER
            string token = username + "-mtcgToken";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return 400;
            }
            try
            {
                using NpgsqlConnection connection = ConnectToDatabase();
                if (!CheckIfUserExists(connection, username))
                {
                    return 401;
                }

                if (!CheckIfTokenIsValid(connection, username, token))
                {
                    Console.WriteLine("Invalid Client Token.");
                    return 401;
                }

                using NpgsqlCommand command = new("SELECT password FROM player WHERE username = @username", connection);
                command.Parameters.AddWithValue("username", username);
                object resultObj = command.ExecuteScalar();

                if (resultObj == null)
                {
                    return 401;
                }

                string result = resultObj?.ToString();

                if (result == password)
                {
                    Console.WriteLine($"{username} logged in successfully!");
                    return 200;
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during login: {e.Message}");
                return 500;
            }

            return 401;
        }

        public static bool CheckIfTokenIsValid(NpgsqlConnection connection, string username, string token)
        {
            try
            {
                using NpgsqlCommand command = new("SELECT @username FROM player WHERE token = @token", connection);
                command.Parameters.AddWithValue("username", username);
                command.Parameters.AddWithValue("token", token);
                object resultObj = command.ExecuteScalar();

                if (resultObj == null)
                {
                    return false;
                }
                return true;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"Failed to connect to Database: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while checking for user: {e.Message}");
                throw;
            }
        }
    }
}