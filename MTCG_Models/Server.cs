using MTCG_Model;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace MTCG_Models
{
    public class Server
    {
        private TcpListener _listener;
        private List<User> _users = new List<User>();


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
                Console.WriteLine("Listening...");

                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    using TcpClient client = _listener.AcceptTcpClient();
                    Console.WriteLine("Connected!");

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
            StreamWriter writer = new StreamWriter(stream);

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
                    ParseRequest(request, writer);      
                    
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
                client.Close();
            }
        }

        public void ParseRequest(string request, StreamWriter writer)
        {
            //  Split the string by each line
            string[] requestLines = request.Split("\r\n");

            // First line of the HTTP request contains the method, path, and HTTP version
            string[] header = requestLines[0].Split(" ");
            string[] secondLine = requestLines[1].Split(" ");

            if (requestLines.Length > 2)
            {
                string[] body = requestLines[2].Split(" ");
            }

            string method = header[0];
            string path = header[1];
            Router(method, path, writer);
        }

        public void Router(string method, string path, StreamWriter writer)
        {
            if (method == "GET")
            {
                Console.WriteLine($"Handling GET Request for {path}...");
                switch (path)
                {
                    case "/":
                        //  Send Success Response Code to Client
                        HTTPResponse(writer, 200);
                        writer.WriteLine("<!DOCTYPE html>");
                        writer.WriteLine("<html><head><title>MTCG - Home</title></head>");
                        writer.WriteLine("<body><h1>Welcome to Monster Trading Cards Game!</h1>");
                        writer.WriteLine("<p>Make sure to sign in! :)</p></body></html>");
                        break;

                    //  Retrieve data from the server
                    case "/login":
                        //  Send Success Response Code to Client
                        HTTPResponse(writer, 200);
                        writer.WriteLine("Redirecting to login.");
                        //  Call login function here
                        break;

                    case "/sessions":
                        //  Missing implementation
                        break;

                    //  Path does not exist
                    default:
                        Console.WriteLine("Invalid HTTP Request Path.");
                        
                        //  Send Error Response Code to Client
                        HTTPResponse(writer, 404);
                        break;
                }
            }
            else if (method == "POST")
            {
                Console.WriteLine($"Handling POST Request for {path}...");
                switch (path)
                {
                    case "/register":
                        //  Send Success Response Code to Client
                        HTTPResponse(writer, 200);
                        writer.WriteLine("Redirecting to registration.");
                        Register(writer);
                        break;

                    case "/users":
                        //  Send Success Response Code to Client
                        HTTPResponse(writer, 200);
                        writer.WriteLine("Redirecting to users.");
                        //Register();
                        break;

                    //  Path does not exist
                    default:
                        Console.WriteLine("Invalid HTTP Request Path.");

                        //  Send Error Response Code to Client
                        HTTPResponse(writer, 404);
                        break;
                }
            }
            else
            {
                //  Method is not allowed
                Console.WriteLine("Method not supported.");
                HTTPResponse(writer, 405);
            }
            
        }

        public void HTTPResponse(StreamWriter writer, int statusCode)
        {
            switch(statusCode)
            {
                case 200:
                    writer.WriteLine("HTTP/1.1 200 OK");
                    writer.WriteLine("Content-Type: text/html");
                    writer.WriteLine();
                    break;

                case 404:
                    writer.WriteLine("HTTP/1.1 404 Not Found");
                    writer.WriteLine("Content-Type: text/html");
                    writer.WriteLine();
                    writer.WriteLine("Invalid HTTP Request Path.");
                    break;

                case 405:
                    writer.WriteLine("HTTP/1.1 405 Method Not Allowed");
                    writer.WriteLine("Content-Type: text/html");
                    writer.WriteLine();
                    writer.WriteLine("Method not supported.");
                    break;
            }
        }

        public bool Register(StreamWriter writer)
        {
            string username = "";
            string password = "";

            /*
            Console.WriteLine("Username: ");
            Console.ReadLine(username);
            Console.WriteLine("Password: ");
            Console.ReadLine(password);
            */

            if (string.IsNullOrWhiteSpace(username))
            {
                writer.WriteLine("Username cannot be empty!");
                return false;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                writer.WriteLine("Password cannot be empty!");
                return false;
            }

            foreach (var user in _users)
            {
                if(username == user.GetUsername)
                {
                    writer.WriteLine("Username already exists!");
                    return false;
                }
            }

            User newUser = new User(username, password);
            _users.Add(newUser);
            writer.WriteLine("Registration successful.");
            return true;
        }

    }
}
