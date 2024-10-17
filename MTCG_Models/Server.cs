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

        //  Method that handles Client Requests
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
                Console.WriteLine("Malformed Request Syntax.");
                HTTPResponse(writer, 400, "Malformed Request Syntax.");
                return;
            }

            Dictionary<string, string> headers = new();

            // First line of the HTTP request contains the HTTP version, method and path
            string[] firstLine = lines[0].Split(" ");
            string method = firstLine[0].Trim();
            string path = firstLine[1].Trim();

            //  Split the header into pairs and add them to the dictionary
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

        //  Method that redirects users depending on the HTTP method
        public void Router(StreamWriter writer, string method, string path, string body)
        {
            switch(method)
            {
                case "GET":
                    GetRequestHandler(writer, path);
                    break;
                case "POST":
                    PostRequestHandler(writer, path, body);
                    break;
                case "PUT":
                    //  Code will be implemented later
                    HTTPResponse(writer, 405, "Method not supported.");
                    break;
                case "DELETE":
                    //  Code will be implemented later
                    HTTPResponse(writer, 405, "Method not supported.");
                    break;
                default:
                    HTTPResponse(writer, 405, "Method not supported.");
                    break;
            }
        }

        //  Method that handles GET Requests
        public void GetRequestHandler(StreamWriter writer, string path)
        {
            Console.WriteLine($"Handling GET Request for {path}...");
            switch (path)
            {
                case "/":
                    HTTPResponse(writer, 200, "Welcome to Monster Trading Cards Game!");
                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    HTTPResponse(writer, 404, "Invalid HTTP Request Path.");
                    break;
            }
        }

        //  Method that handles POST Requests
        public void PostRequestHandler(StreamWriter writer, string path, string body)
        {
            Console.WriteLine($"Handling POST Request for {path}...");

            //  Parse the body to get username and password                        
            var data = ParseBody(writer, body);
            if(data == null)
            {
                return;
            }

            int statusCode;
            switch (path)
            {
                //API-Endpoint for sign up
                case "/users":

                    Console.WriteLine($"Redirecting to {path}.");
                    statusCode = Register(data);
                    Console.WriteLine(statusCode);

                    if (statusCode == 201)
                    {
                        HTTPResponse(writer, statusCode, "User created successfully.");
                    }
                    else if (statusCode == 400)
                    {
                        HTTPResponse(writer, statusCode, "Username and password are required.");
                    }
                    else
                    {
                        HTTPResponse(writer, statusCode, "Username already exists.");
                    }

                    break;

                //  API-Endpoint for login
                case "/sessions":

                    Console.WriteLine($"Redirecting to {path}.");
                    statusCode = Login(data);
                    Console.WriteLine(statusCode);

                    if (statusCode == 200)
                    {
                        HTTPResponse(writer, statusCode, "User login successful.");
                    }
                    else
                    {
                        HTTPResponse(writer, statusCode, "Invalid username / password provided");
                    }

                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    Console.WriteLine("Path invalid.");
                    HTTPResponse(writer, 404, "Invalid HTTP Request Path.");
                    break;
            }
        }

        //  Method to parse the body from JSON to a dictionary pair
        public Dictionary<string, string> ParseBody(StreamWriter writer, string body)
        {
            //  If body contains nothing in it, output an error code
            if(string.IsNullOrWhiteSpace(body))
            {
                HTTPResponse(writer, 400, "Request Body is empty.");
            }
            
            //  Save the data in a dictionary
            var data = new Dictionary<string, string>();
            try
            {
                data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
            }
            catch (Exception e)
            {
                Console.WriteLine("Parsing body failed." + e.Message);
                HTTPResponse(writer, 400, "Parsing body failed.");
            }
            return data;
        }

        //  Method that handles registering a new user
        public int Register(Dictionary<string, string> data)
        {
            string username = data["Username"];
            string password = data["Password"];

            //  If username or password are empty, return 
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                return 400;
            }

            //  Loop through the user list to check if the username is already in use
            foreach (var user in _users)
            {
                if(username == user.GetUsername)
                {
                    return 409;
                }
            }
            
            //  Create a new user and add them to the list
            User newUser = new(username, password);
            _users.Add(newUser);
            Console.WriteLine(newUser.GetUsername);
            Console.WriteLine(newUser.GetPassword);
            return 201;
        }

        //  Method that handles login from already registered Users
        public int Login(Dictionary<string, string> data)
        {
            string username = data["Username"];
            string password = data["Password"];

            if (!(string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(password)))
            {
                //  Loop through the user list to find the username and check if it matches the password
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

        //  Method that is responsible for handling the correct HTTP status messages
        public void HTTPResponse(StreamWriter writer, int statusCode, string message)
        {
            //  Send Response back to client based on the status code
            switch (statusCode)
            {
                case 200:
                    writer.WriteLine($"HTTP/1.1 {statusCode} OK");
                    break;

                case 201:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Created");
                    break;

                case 400:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Bad Request");
                    break;

                case 401:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Unauthorized");
                    break;

                case 404:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Not Found");
                    break;

                case 405:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Method Not Allowed");
                    break;

                case 409:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Conflict");
                    break;

                //  Unexpected status codes
                default:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Unknown Status");
                    message = "Client requested unknown status.";
                    break;
            }

            writer.WriteLine("Content-Type: application/json");
            writer.WriteLine();
            writer.WriteLine(message);
        }
    }
}
