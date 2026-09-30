using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new HttpServer();
        
        Task serverTask = server.StartAsync();

        Console.WriteLine("Введите 'stop' в консоли, чтобы остановить сервер...");
        
        while (true)
        {
            string? command = Console.ReadLine();
            
            if (command?.ToLower() == "stop")
            {
                server.Stop();
                break;
            }
        }
        
        await serverTask;
    }
}