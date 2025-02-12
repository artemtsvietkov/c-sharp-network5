using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    private static List<Process> clients = new List<Process>();
    private static TcpListener listener;
    private static bool isRunning = true;

    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "client")
        {
            // Если запущен как клиент
            RunAsClient();
            return;
        }

        Console.WriteLine("Startar servern...");

        // Создаем TCP-слушателя на порту 13000
        listener = new TcpListener(System.Net.IPAddress.Loopback, 13000);
        listener.Start();
        Console.WriteLine("Servern har startats. Väntar på anслутningar...");

        // Запускаем поток для приема новых клиентов
        Thread acceptThread = new Thread(AcceptClients);
        acceptThread.Start();

        // Создаем несколько реальных клиентов в отдельных консольных окнах
        for (int i = 1; i <= 3; i++)
        {
            Process clientProcess = StartClientInNewConsole(i);
            clients.Add(clientProcess);
        }

        // Главный цикл для ввода сообщений из консоли
        while (isRunning)
        {
            Console.Write("Skriv ett meddelande: ");
            string message = Console.ReadLine();

            if (message.ToLower() == "exit")
            {
                Console.WriteLine("Stänger servern...");
                isRunning = false;
                break;
            }

            BroadcastMessage(message);
        }

        // Остановка сервера
        listener.Stop();
        foreach (Process client in clients)
        {
            client.Kill(); // Завершаем процессы клиентов
        }
    }

    // Метод для приема новых клиентов
    private static void AcceptClients()
    {
        while (isRunning)
        {
            try
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Ny klient ansluten!");
            }
            catch
            {
                break; // Если сервер останавливается, выходим из цикла
            }
        }
    }

    // Метод для запуска клиента в новом консольном окне с помощью dotnet
    private static Process StartClientInNewConsole(int clientId)
    {
        Process process = new Process();
        process.StartInfo.FileName = "cmd.exe"; // Запускаем командную строку
        process.StartInfo.Arguments = $"/k title Klient {clientId} && echo Klient {clientId} ansluten till servern. && timeout /t 2 >nul && dotnet run --project . --client";
        process.StartInfo.CreateNoWindow = false; // Открываем новое окно
        process.Start();
        return process;
    }

    // Метод для трансляции сообщения всем клиентам
    private static void BroadcastMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        foreach (Process client in clients)
        {
            // Здесь можно добавить логику для отправки сообщений каждому клиенту
            Console.WriteLine($"Skickar meddelande till alla klienter: {message}");
        }
    }

    // Метод для работы как клиент
    private static void RunAsClient()
    {
        try
        {
            TcpClient client = new TcpClient("127.0.0.1", 13000);
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            Console.WriteLine("Klient ansluten till servern.");

            // Ждём сообщения от сервера
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).TrimEnd('\0');
                Console.WriteLine($"Mottaget från servern: {message}");
            }

            stream.Close();
            client.Close();
        }
        catch
        {
            Console.WriteLine("Klienten avslutas.");
        }
    }
}