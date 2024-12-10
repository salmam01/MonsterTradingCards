using MonsterTradingCardsGame.MTCG_Models.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class Routing
    {
        //  Method that redirects users depending on the HTTP method
        /*public static void Router(StreamWriter writer, string method, string path, string body)
        {
            switch (method)
            {
                case "GET":
                    GetRequestHandler(writer, path);
                    break;

                case "POST":
                    PostRequestHandler(writer, path, body);
                    break;

                case "PUT":
                    //  Code will be implemented later
                    HTTPResponse.Response(writer, 405, "Method not supported.");
                    break;

                case "DELETE":
                    //  Code will be implemented later
                    HTTPResponse.Response(writer, 405, "Method not supported.");
                    break;

                default:
                    HTTPResponse.Response(writer, 405, "Method not supported.");
                    break;
            }
        }

        public static void GetRequestHandler(StreamWriter writer, string path)
        {
            Console.WriteLine($"Handling GET Request for {path}...");
            switch (path)
            {
                case "/":
                    HTTPResponse.Response(writer, 200, "Welcome to Monster Trading Cards Game!");
                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    HTTPResponse.Response(writer, 404, "Invalid HTTP Request Path.");
                    break;
            }
        }

        public static void PostRequestHandler(StreamWriter writer, string path, string body)
        {
            Console.WriteLine($"Handling POST Request for {path}...");

            //  Parse the body to get username and password
            var data = Parser.RegisterParser(writer, body);
            if (data == null)
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
                        HTTPResponse.Response(writer, statusCode, "User created successfully.");
                    }
                    else if (statusCode == 400)
                    {
                        HTTPResponse.Response(writer, statusCode, "Username and password are required.");
                    }
                    else if (statusCode == 500)
                    {
                        HTTPResponse.Response(writer, statusCode, "Server was unable to connect to database.");
                    }
                    else
                    {
                        HTTPResponse.Response(writer, statusCode, "Username already exists.");
                    }

                    break;

                //  API-Endpoint for login
                case "/sessions":

                    Console.WriteLine($"Redirecting to {path}.");
                    statusCode = Login(data);
                    Console.WriteLine(statusCode);

                    if (statusCode == 200)
                    {
                        HTTPResponse.Response(writer, statusCode, "Login successful.");
                    }
                    else
                    {
                        HTTPResponse.Response(writer, statusCode, "Invalid username / password provided");
                    }

                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    Console.WriteLine("Path invalid.");
                    HTTPResponse.Response(writer, 404, "Invalid HTTP Request Path.");
                    break;
            }
        }
        */
    }
}
