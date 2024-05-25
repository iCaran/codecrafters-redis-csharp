using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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

        while (true)
        {
            // Accept a client connection
            Socket clientSocket = server.AcceptSocket();
            Console.WriteLine("Client connected.");

            // Handle the client connection in a new thread
            Thread clientThread = new Thread(() => HandleClient(clientSocket));
            clientThread.Start();
        }
    }

    static void HandleClient(Socket clientSocket)
    {
        try
        {
            // NetworkStream to read and write data
            using (NetworkStream networkStream = new NetworkStream(clientSocket))
            using (StreamReader reader = new StreamReader(networkStream, Encoding.UTF8))
            using (StreamWriter writer = new StreamWriter(networkStream, new UTF8Encoding(false)) { NewLine = "\r\n", AutoFlush = true })
            {
                // Continuously read commands from the client
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        break; // Client disconnected
                    }
                    Console.WriteLine($"Received command: {line}");

                    // Check if it's the start of a RESP array
                    if (line.StartsWith("*"))
                    {
                        int numberOfElements = int.Parse(line.Substring(1));
                        string[] elements = new string[numberOfElements];

                        for (int i = 0; i < numberOfElements; i++)
                        {
                            string bulkStringLength = reader.ReadLine(); // Read $N
                            elements[i] = reader.ReadLine();             // Read actual string
                        }

                        string command = elements[0].ToUpper();
                        if (command == "PING")
                        {
                            // Send +PONG\r\n response
                            writer.WriteLine("+PONG");
                            Console.WriteLine("Sent response: +PONG");
                        }
                        else if (command == "ECHO" && numberOfElements > 1)
                        {
                            string message = elements[1];
                            string response = $"${message.Length}\r\n{message}";
                            // Send bulk string response
                            writer.WriteLine(response);
                            Console.WriteLine($"Sent response: {response}");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception: {e.Message}");
        }
        finally
        {
            clientSocket.Close();
        }
    }
}
