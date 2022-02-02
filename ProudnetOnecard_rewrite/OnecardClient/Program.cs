using Nettention.Proud;
using OnecardCommon;

namespace Client
{
    class Program
    {
        static object g_critSec = new object();
        static NetClient netClient = new NetClient();
        static S2C.Stub S2CStub = new S2C.Stub();
        static C2S.Proxy C2SProxy = new C2S.Proxy();
        static bool isConnected = false;
        static bool isLoggedin = false;
        static bool isPlaying = false;
        static bool keepWorkerThread = true;

        static User me = new User();

        static void InitializeStub()
        {
            S2CStub.ResponseLogin = (HostID remote, RmiContext rmiContext, User user) =>
            {
                return true;
            };

            S2CStub.ResponseEnter = (HostID remote, RmiContext rmiContext, int RoomNumber, int playerID) =>
            {
                return true;
            };
            S2CStub.ResponseDraw = (HostID remote, RmiContext rmiContext, List<GameCard> hand) =>
            {
                return true;
            };

            S2CStub.ChangeHand = (HostID remote, RmiContext rmiContext,int playerID, int count) =>
            {
                return true;
            };
            S2CStub.ChangeLastCard = (HostID remote, RmiContext rmiContext, GameCard card) =>
            {
                return true;
            };
            S2CStub.ChangeTurn = (HostID remote, RmiContext rmiContext, int playerID) =>
            {
                return true;
            };

            S2CStub.NotifyStartGame = (HostID remote, RmiContext rmiContext, int firstPlayer, GameCard firstCard) =>
            {
                return true;
            };
            S2CStub.NotifyEndGame = (HostID remote, RmiContext rmiContext, int winnerID) =>
            {
                return true;
            };
        }
        static void InitializeHandler()
        {
            netClient.JoinServerCompleteHandler = (info, replyFromServer) =>
            {
                lock (g_critSec)
                {
                    if (info.errorType == ErrorType.Ok)
                    {
                        Console.Write("Succeed to connect server. Allocated hostID={0}\n", netClient.GetLocalHostID());
                        isConnected = true;
                    }
                    else
                    {
                        Console.Write("Failed to connect server.\n");
                        Console.WriteLine("errorType = {0}, detailType = {1}, comment = {2}", info.errorType, info.detailType, info.comment);
                    }
                }
            };

            netClient.LeaveServerHandler = (errorInfo) =>
            {
                lock (g_critSec)
                {
                    Console.Write("OnLeaveServer: {0}\n", errorInfo.comment);

                    isConnected = false;
                    keepWorkerThread = false;
                }
            };

        }
        static void initializeClient()
        {
            netClient.AttachStub(S2CStub);
            netClient.AttachProxy(C2SProxy);
        }
        static void InitializeClientParameter()
        {
            NetConnectionParam cp = new NetConnectionParam();
            cp.protocolVersion.Set(Vars.m_Version);
            cp.serverIP = "localhost";
            cp.serverPort = (ushort)Vars.m_serverPort;
            netClient.Connect(cp);
        }
        static void Draw()
        {
            Console.Clear();
        }

        static void printHand(int playerID)
        {
        }
        static void PlayCard()
        {
        }
        static void Main(string[] args)
        {
            InitializeHandler();
            initializeClient();
            InitializeStub();
            InitializeClientParameter();

            Thread workerThread = new Thread(() =>
            {
                while (keepWorkerThread)
                {
                    Thread.Sleep(10);
                    netClient.FrameMove();
                }
            });
            workerThread.Start();

            // Connection
            while (!isConnected)
                Thread.Sleep(1000);

            // Login
            while (!isLoggedin)
            {
                Console.Write("UserName: ");
                string userInput = Console.ReadLine();
                if (userInput == "")
                    continue;

                Console.WriteLine("Login...");
                C2SProxy.Login(HostID.HostID_Server, RmiContext.ReliableSend, userInput);
                Thread.Sleep(1000);
            }

            // Enter to game room
            while(me.RoomNumber == 0)
            {
                string userInput = Console.ReadLine();
                if (userInput == "")
                    continue;
                try
                {
                    int RoomNumber = Int32.Parse(userInput);
                    C2SProxy.EnterRoom(HostID.HostID_Server, RmiContext.ReliableSend, RoomNumber);
                }
                catch (FormatException ex)
                {

                }
                Thread.Sleep(1000);
            }

            // Playing
            ConsoleKeyInfo keyinfo;
            int i = 1;
            while (keepWorkerThread)
            {
                if (Console.KeyAvailable)
                {
                   
                }
                System.Threading.Thread.Sleep(30);
            }

            workerThread.Join();
            netClient.Disconnect();
        }
    }
}