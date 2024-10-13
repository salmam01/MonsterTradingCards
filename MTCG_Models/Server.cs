using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MTCG_Models
{
    public class Server
    {
        private TcpListener listener;
        private readonly string url;

        public Server(string url) 
        {
            this.url = url;
            var uri = new Uri(url);
            int port = uri.Port;
            listener = new TcpListener(IPAddress.Any, port);
        }

        public void StartServer()
        {
            try
            {
                listener.Start();
                Console.WriteLine("Listening...");

                while (true)
                {
                    using TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    //  Pass handling the client requests to the function
                    RequestHandler(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                listener.Stop();
            }           
        }

        public void RequestHandler(TcpClient client)
        {
            // Get a stream object for reading and writing
            using NetworkStream stream = client.GetStream();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            string request;

            int bytesRead;
            while ((bytesRead = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                request = Encoding.UTF8.GetString(bytes, 0, bytesRead);
                Console.WriteLine("Received: {0}", request);

                // Send back a response.
                switch(request)
                {
                    //  Retrieve data from the server
                    case "GET":
                        Console.WriteLine("Handling GET Request...");
                        break;

                    //  Send data to server, create or update resources
                    case "POST":
                        Console.WriteLine("Handling POST Request...");
                        break;
                    default:
                        Console.WriteLine("Invalid HTTP Request...");
                        break;
                }
            }
        }
    }
}
