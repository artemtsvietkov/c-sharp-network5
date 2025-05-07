using System.Net.Sockets;
using System.Text;

try
{
    var client = new TcpClient("127.0.0.1", 13001);
    var stream = client.GetStream();
    byte[] buffer = new byte[1024];
    Console.WriteLine("Connected to the server.");

    while (true)
    {
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        if (bytesRead == 0) break;

        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine("Message from the server: " + message);
    }
}
catch
{
    Console.WriteLine("Failed to connect to the server.");
}
