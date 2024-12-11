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

        //  REWORK THIS SHIT PLS
        //  Method that handles POST Requests
        public static void PostRequestHandler(StreamWriter writer, Dictionary<string, string> request)
        {
            string path = request["Path"];
            Console.WriteLine($"Handling POST Request for {path}...");

            int statusCode;
            string token = null;
            switch (path)
            {
                //  API-Endpoint for sign up
                case "/users":

                    Console.WriteLine($"Redirecting to {path}.");
                    statusCode = UserManagement.Register(request);
                    Console.WriteLine($"Status: {statusCode}");

                    switch(statusCode)
                    {
                        case 201:
                            HTTPResponse.Response(writer, statusCode, "User created successfully.");
                            break;

                        case 400:
                            HTTPResponse.Response(writer, statusCode, "Username and password are required.");
                            break;

                        case 409:
                            HTTPResponse.Response(writer, statusCode, "Username already exists.");
                            break;

                        default:
                            HTTPResponse.Response(writer, statusCode, "Internal Server error occured.");
                            break;
                    }

                    break;

                //  API-Endpoint for login
                case "/sessions":

                    Console.WriteLine($"Redirecting to {path}.");
                    statusCode = UserManagement.Login(request);
                    Console.WriteLine(statusCode);

                    switch (statusCode)
                    {
                        case 200:
                            TokenManagement.SendTokenToClient(writer, statusCode, "Login successful.", token);
                            break;

                        case 400:
                            HTTPResponse.Response(writer, statusCode, "Username or Password cannot be empty.");
                            break;

                        case 401:
                            HTTPResponse.Response(writer, statusCode, "Invalid Client Authentication Token.");
                            break;

                        case 404:
                            HTTPResponse.Response(writer, statusCode, "Incorrect Username or Password.");
                            break;

                        default:
                            HTTPResponse.Response(writer, statusCode, "Internal Server Error occured.");
                            break;
                    }
                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    Console.WriteLine("Path invalid.");
                    HTTPResponse.Response(writer, 404, "Invalid HTTP Request Path");
                    break;
            }
        }

    }
}
