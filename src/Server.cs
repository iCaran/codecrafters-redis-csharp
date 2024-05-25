using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class RedisServer
{
    static void Main()
    {
        // Debugging logs
        Console.WriteLine("Logs from your program will appear here!");

        // Set up the server to listen on any IP address, port 6379
        TcpListener server = new TcpListener(IPAddress.Any, 6379);
        server.Start();
        Console.WriteLine("Server started, waiting for connections...");

        // Accept a client connection
        using (Socket clientSocket = server.AcceptSocket())
        {
            Console.WriteLine("Client connected.");

            // NetworkStream to read and write data
            using (NetworkStream networkStream = new NetworkStream(clientSocket))
            using (StreamReader reader = new StreamReader(networkStream, Encoding.UTF8))
            using (StreamWriter writer = new StreamWriter(networkStream, new UTF8Encoding(false)) { NewLine = "\r\n", AutoFlush = true })
            {
                // Read the incoming RESP data
                string line = reader.ReadLine();
                Console.WriteLine($"Received command: {line}");

                // Check if it's the start of a RESP array
                if (line != null && line.StartsWith("*"))
                {
                    int numberOfElements = int.Parse(line.Substring(1));
                    for (int i = 0; i < numberOfElements; i++)
                    {
                        string bulkStringLength = reader.ReadLine(); // Read $4
                        string command = reader.ReadLine();          // Read PING

                        Console.WriteLine($"Received bulk string length: {bulkStringLength}");
                        Console.WriteLine($"Received command: {command}");

                        if (command == "PING")
                        {
                            // Send +PONG\r\n response
                            writer.WriteLine("+PONG");
                            Console.WriteLine("Sent response: +PONG");
                        }
                    }
                }
            }
        }
    }
}
