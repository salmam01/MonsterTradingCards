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
        private readonly string _url;
        private List<User> _users = new List<User>();


        public Server(string url) 
        {
            this._url = url;
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
            // Get a stream object for reading and writing
            using NetworkStream stream = client.GetStream();

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

                    //  Split the string
                    string[] requestLines = request.Split("\r\n");

                    // First line of the HTTP request contains the method, path, and HTTP version
                    string[] requestLineParts = requestLines[0].Split(' ');

                    string method = requestLineParts[0];
                    string path = requestLineParts[1];

                    Router(method, path);
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

        public void Router(string method, string path)
        {
            if (method == "GET")
            {
                switch (path)
                {
                    //  Retrieve data from the server
                    case "/login":
                        Console.WriteLine($"Handling GET Request for {path}...");
                        //  Call login function here
                        break;

                    //  Path does not exist
                    default:
                        Console.WriteLine("Invalid HTTP Request Path.");
                        break;
                }
            }
            else if (method == "POST")
            {
                switch (path)
                {
                    case "/register":
                        Console.WriteLine($"Handling POST Request for {path}...");
                        Register();
                        break;

                    //  Path does not exist
                    default:
                        Console.WriteLine("Invalid HTTP Request Path.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Method not supported.");

            }
            
        }

        public bool Register()
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
                Console.WriteLine("Username cannot be empty!");
                return false;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Password cannot be empty!");
                return false;
            }

            foreach (var user in _users)
            {
                if(username == user.GetUsername)
                {
                    Console.WriteLine("Username already exists!");
                    return false;
                }
            }

            User newUser = new User(username, password);
            _users.Add(newUser);
            Console.WriteLine("Registration successful.");
            return true; 
        }

    }
}
