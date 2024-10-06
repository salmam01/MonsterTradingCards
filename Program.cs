// See https://aka.ms/new-console-template for more information
using MTCG_Models;
using System.Net.Security;

Server server = new Server("http://localhost:10001");
server.StartServer();