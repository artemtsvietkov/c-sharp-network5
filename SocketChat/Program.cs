using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;

var listener = new TcpListener(IPAddress.Loopback, 13001);
listener.Start();
Console.WriteLine("Сервер запущен. Ожидание подключений...");

var clients = new List<TcpClient>();

_ = Task.Run(async () =>
{
    while (true)
    {
        var client = await listener.AcceptTcpClientAsync();
        clients.Add(client);
        Console.WriteLine("Клиент подключен.");
    }
});

while (true)
{
    string? input = Console.ReadLine();
    if (input == null) continue;
    if (input.ToLower() == "exit") break;

    var buffer = Encoding.UTF8.GetBytes(input);
    foreach (var client in clients.ToArray())
    {
        try
        {
            var stream = client.GetStream();
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch
        {
            clients.Remove(client);
        }
    }
}
