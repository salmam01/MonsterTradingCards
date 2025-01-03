// See https://aka.ms/new-console-template for more information
using MonsterTradingCardsGame.Server;
using System.Net.Security;

Server server = new Server("http://localhost:10001");
await server.StartServer();