using System;

namespace HttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var httpServer = new HttpServer("./Settings.json");

            Console.WriteLine("[ Доступные команды: start, stop, restart. ]");
            PrintMessage("Сервер готов к запуску.");
            while (true)
            {
                var input = Console.ReadLine();
                recognizeCommand(httpServer, input).Invoke();
            }
        }
        internal static void PrintMessage(string message)
        {
            Console.WriteLine($"\n >>> {message}");
        }

        private static Action recognizeCommand(HttpServer httpServer, string command)
        {
            switch (command)
            {
                case "start":
                    return new Action(httpServer.Start);
                case "stop":
                    return new Action(httpServer.Stop);
                case "restart":
                    return new Action(() => { httpServer.Stop(); httpServer.Start(); });
                default:
                    PrintMessage("Неверная команда.");
                    return new Action(() => { });
            }
        }
    }

    
}