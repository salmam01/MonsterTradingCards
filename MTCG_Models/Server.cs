using MTCG_Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static System.Runtime.InteropServices.JavaScript.JSType;

/*
Curl Command:
curl -X POST http://localhost:10001/users -H "Content-Type: application/json" -d "{\"Username\": \"newuser\", \"Password\": \"password123\"}"
*/

namespace MTCG_Models
{
    public class Server
    {
        private TcpListener _listener;
        private List<User> _users = new();


        public Server(string url) 
        {
            var uri = new Uri(url);
            int port = uri.Port;
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public void StartServer()
        {
            try
            {
                _listener.Start();
                Console.WriteLine("Waiting for a connection...");
                while (true)
                {
                    using TcpClient client = _listener.AcceptTcpClient();
                    Console.WriteLine("New client connected!");

                    //  Pass handling the client requests to the function
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

        public void RequestHandler(TcpClient client)
        {
            // Get a network stream object for reading from and writing to the client
            using NetworkStream stream = client.GetStream();
            StreamWriter writer = new(stream);

            // Buffer for reading data
            Byte[] bytes = new Byte[1024];
            string request;

            try
            {
                int bytesRead;
                while ((bytesRead = stream.Read(bytes, 0, bytes.Length)) != 0)
                {                  
                    // Translate data bytes to an ASCII string.
                    request = Encoding.UTF8.GetString(bytes, 0, bytesRead);
                    Console.WriteLine($"Received: {request}");
                    ParseRequest(writer, request);      
                    
                    //  Output response
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

        public void ParseRequest(StreamWriter writer, string request)
        {
            //  Split the request string by each line
            string[] lines = request.Split("\r\n");
            if (lines.Length == 0)
            {
                Console.WriteLine("Invalid HTTP Request.");
                //HTTPResponse(writer, 400);
                writer.WriteLine("Malformed Request Syntax.");
                return;
            }

            Dictionary<string, string> headers = new Dictionary<string, string>();

            // First line of the HTTP request contains the HTTP version, method and path
            string[] firstLine = lines[0].Split(" ");
            string method = firstLine[0].Trim();
            string path = firstLine[1].Trim();

            int i = 1;
            while (i < lines.Length && lines[i] != "")
            {              
                string[] headerParts = lines[i].Split(':', 2);

                if (headerParts.Length == 2)
                {
                    headers[headerParts[0].Trim()] = headerParts[1].Trim();
                }
                i++;
            }
            i++;

            string body = string.Join("\r\n", lines.Skip(i).ToArray());
            Console.WriteLine($"Body: {body}");
            
            Router(writer, method, path, body);
        }

        public void Router(StreamWriter writer, string method, string path, string body)
        {
            string message;
            if (method == "GET")
            {
                Console.WriteLine($"Handling GET Request for {path}...");
                switch (path)
                {
                    case "/":
                        message = "Welcome to Monster Trading Cards Game!";
                        HTTPResponse(writer, 200, message);
                        break;

                    //  Path does not exist, send Error Response Code to Client
                    default:
                        message = "Invalid HTTP Request Path.";
                        HTTPResponse(writer, 404, message);
                        break;
                }
            }
            else if (method == "POST")
            {
                Console.WriteLine($"Handling POST Request for {path}...");

                //  Parse the body to get username and password                        
                var data = ParseBody(writer, body);
                int statusCode;

                switch (path)
                {
                    case "/users":

                        statusCode = Register(data);
                        Console.WriteLine(statusCode);

                        if (statusCode == 200)
                        {
                            message = "User created successfully.";
                        }
                        else if (statusCode == 400)
                        {
                            message = "Username and password are required.";
                        }
                        else
                        {
                            message = "Username already exists.";
                        }

                        HTTPResponse(writer, statusCode, message);
                        break;

                    //  Retrieve data from the server
                    case "/sessions":

                        Console.WriteLine("Redirecting to login.");
                        statusCode = Login(data);
                        
                        if (statusCode == 200)
                        {
                            message = "User login successful.";
                        }
                        else
                        {
                            message = "Invalid username / password provided";
                        }

                        HTTPResponse(writer, statusCode, message);
                        break;

                    //  Path does not exist
                    default:
                        message = ("Invalid HTTP Request Path.");
                        HTTPResponse(writer, 404, message);
                        break;
                }
            }
            else if (method == "PUT")
            {
                //  Code will be implemented later
            }
            else if (method == "DELETE")
            {
                //  Code will be implemented later
            }
            else
            {
                //  Method is not allowed
                message = "Method not supported.";
                HTTPResponse(writer, 405, message);
            }
        }

        public Dictionary<string, string> ParseBody(StreamWriter writer, string body)
        {
            if(body == null)
            {
                //throw Exception()
                // problem idk the fix to
            }
            var data = new Dictionary<string, string>();
            try
            {
                data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
            }
            catch (Exception e)
            {
                Console.WriteLine("Parsing body failed." + e.Message);
                HTTPResponse(writer, 400, e.Message);
            }
            return data;
        }

        public void HTTPResponse(StreamWriter writer, int statusCode, string message)
        {
            //  Send Response back to client based on the status code
            switch(statusCode)
            {
                case 200:
                    writer.WriteLine($"HTTP/1.1 {statusCode} OK");
                    writer.WriteLine("Content-Type: text/html");
                    writer.WriteLine();
                    writer.WriteLine(message);
                    break;

                case 201:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Created");
                    writer.WriteLine("Content-Type: application/json");
                    writer.WriteLine();
                    writer.WriteLine(message);
                    break;

                case 400:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Bad Request");
                    writer.WriteLine("Content-Type: application/json");
                    writer.WriteLine();
                    writer.WriteLine(message);
                    break;

                case 401:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Unauthorized");
                    writer.WriteLine("Content-Type: application/json");
                    writer.WriteLine();
                    writer.WriteLine(message);
                    //writer.WriteLine("Invalid username/password provided.");
                    break;

                case 404:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Not Found");
                    writer.WriteLine("Content-Type: application/json");
                    writer.WriteLine();
                    writer.WriteLine(message);
                    //writer.WriteLine("Invalid HTTP Request Path.");
                    break;

                case 405:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Method Not Allowed");
                    writer.WriteLine("Content-Type: application/json");
                    writer.WriteLine();
                    writer.WriteLine(message);
                    //writer.WriteLine("Method not supported.");
                    break;

                case 409:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Conflict");
                    writer.WriteLine("Content-Type: application/json");
                    writer.WriteLine();
                    writer.WriteLine(message);
                    break;

                //  Unexpected status codes
                default:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Unknown Status");
                    writer.WriteLine("Content-Type: application/json");
                    writer.WriteLine();
                    writer.WriteLine("Client requested unknown status.");
                    break;
            }
        }

        public int Register(Dictionary<string, string> data)
        {
            string username = data["Username"];
            string password = data["Password"];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                return 400;
            }

            foreach (var user in _users)
            {
                if(username == user.GetUsername)
                {
                    return 409;
                }
            }

            User newUser = new(username, password);
            _users.Add(newUser);
            Console.WriteLine(newUser.GetUsername);
            Console.WriteLine(newUser.GetPassword);
            return 200;
        }

        public int Login(Dictionary<string, string> data)
        {
            string username = data["Username"];
            string password = data["Password"];

            if (!(string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(password)))
            {
                foreach (var user in _users)
                {
                    if (username == user.GetUsername)
                    {
                        if (password == user.GetPassword)
                        {
                            return 200;
                        }
                    }
                }
            }
            return 401;
        }

    }
}
