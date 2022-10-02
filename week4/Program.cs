using System;
using System.Net;
using System.IO;

namespace NetConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string responseStr = File.ReadAllText(@"C:\Users\arsha\source\repos\inf 2022-2023\HttpServer\google.html");
            var httpServer = new HttpServer("http://localhost:8888/", responseStr);
        }
    }

    class HttpServer
    {
        private readonly HttpListener listener;
        private string responseString;
        private bool isWorking;

        public HttpServer(string url, string responseString)
        {
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            this.responseString = responseString;
            Console.WriteLine("Сервер настроен.");
            Start();
        }

        private void Start()
        { 
            listener.Start();
            isWorking = true;

            Console.WriteLine("\n\nСервер запущен." +
                "\n\t>>> stop  <<< Остановить сервер\n");

            Thread thread = new Thread(Processing);
            thread.Start();

            CheckCommand(new Dictionary<string, Action>
            {
                {"stop", Stop},
            });
        }

        private void Stop()
        {
            listener.Stop();
            isWorking = false;

            Console.WriteLine("\n\nСервер остановлен." +
                "\n\t>>> start <<< Запустить сервер" +
                "\n\t>>> exit  <<< Выйти\n");


            CheckCommand(new Dictionary<string, Action>
            {
                {"start", Start},
                {"exit", new Action(() => { }) }
            });
        }

        private void CheckCommand(Dictionary<string, Action> commands)
        {
            while (true)
            {
                Console.Write(">>> ");
                var input = Console.ReadLine();
                Action action;
                if (commands.TryGetValue(input, out action))
                {
                    action();
                    break;
                }
                else
                    Console.WriteLine("Неверная команда\n");
            }
        }

        private void Processing()
        {
            HttpListenerContext context;
            try
            {
                context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
                Processing();
            }
            catch (Exception ex)
            {
                if (isWorking)
                {
                    Console.WriteLine("Произошла ошибка: " + ex.Message);
                    Stop();
                }
                return;
            }
        }
    }
}