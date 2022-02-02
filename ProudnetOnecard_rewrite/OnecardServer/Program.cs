using OnecardCommon;

namespace OnecardServer
{

    class Program
    {
        static void Main()
        {
            ServerLauncher server = new ServerLauncher();

            try
            {
                server.ServerStart();
                Console.Write("Server started\n");
                while (server.RunLoop)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape && Console.ReadKey(true).Key == ConsoleKey.Delete)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(1000);
                }

                Console.Write("Server Closed\n");
                server.Dispose();

            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
        }
    }
}