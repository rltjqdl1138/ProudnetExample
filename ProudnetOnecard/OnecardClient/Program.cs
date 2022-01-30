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

        static int Selected = 0;
        static int turn;
        static int PlayerID;
        static GameCard LastCard = new GameCard();
        static GamePlayer[] players = new GamePlayer[4];
        static List<GameCard> gameCards = new List<GameCard>();

        static void InitializeStub()
        {
            S2CStub.ResponseLogin = (HostID remote, RmiContext rmiContext, User user) =>
            {
                lock (g_critSec)
                {
                    isLoggedin = true;
                    me = user;
                }
                return true;
            };

            S2CStub.ResponseEnter = (HostID remote, RmiContext rmiContext, int RoomNumber, int playerID) =>
            {
                lock (g_critSec)
                {
                    if(RoomNumber == 0)
                    {
                        Console.WriteLine("Cannot Enter the room");
                    }
                    else
                    {
                        Console.WriteLine("Enter Room {0}", RoomNumber);
                        me.RoomNumber = RoomNumber;
                        PlayerID = playerID;
                    }
                }
                return true;
            };

            S2CStub.NotifyStart = (HostID remote, RmiContext rmiContext, int firstPlayer, GameCard firstCard) =>
            {
                lock (g_critSec)
                {
                    isPlaying = true;
                    turn = firstPlayer;
                    LastCard = firstCard;

                    for (int i = 0; i < 4; i++)
                    {
                        players[i] = new GamePlayer();
                        for(int j=0; j<7; j++)
                            players[i].hand.Append(new GameCard());
                    }

                    Console.WriteLine("Game Start!");
                    printHand();
                }
                return true;
            };
            S2CStub.ResponseDraw = (HostID remote, RmiContext rmiContext, List<GameCard> hand) =>
            {

                lock (g_critSec)
                {
                    for (int i = 0; i < hand.Count; i++)
                    {
                        GameCard card = hand[i];
                        
                        gameCards.Add(card);
                    }
                }
                return true;
            };
            S2CStub.ChangeHand = (HostID remote, RmiContext rmiContext,int playerID, int count) =>
            {
                players[playerID].hand = new List<GameCard>(count);
                return true;
            };
            S2CStub.ChangeLastCard = (HostID remote, RmiContext rmiContext, GameCard card) =>
            {
                LastCard = card;
                if(gameCards[Selected].toNumber() == card.toNumber())
                    gameCards.RemoveAt(Selected);

                Console.WriteLine("Card:{0}\n", LastCard.toString());
                return true;
            };
            S2CStub.ChangeTurn = (HostID remote, RmiContext rmiContext, int playerID) =>
            {
                turn = playerID;
                if (turn == PlayerID)
                {
                    Console.WriteLine("My turn!");
                    printHand();
                }
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
        static void printHand()
        {
            for(int i=0; i<gameCards.Count; i++)
                Console.WriteLine("{0}: {1}",i+1, gameCards[i].toString());
            
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
            while (keepWorkerThread)
            {

                keyinfo = Console.ReadKey();
                if (!isPlaying)
                {
                    C2SProxy.Start(HostID.HostID_Server, RmiContext.ReliableSend);
                    continue;
                }
                Console.WriteLine("{0} {1}", turn, PlayerID);
                if(turn != PlayerID)
                {
                    Console.WriteLine("Not my turn");
                }

                switch (keyinfo.Key)
                {
                    case ConsoleKey.D1:
                        Selected = 0;
                        C2SProxy.PlayCard(HostID.HostID_Server, RmiContext.ReliableSend, gameCards[Selected]);
                        break;
                    case ConsoleKey.D2:
                        Selected = 1;
                        C2SProxy.PlayCard(HostID.HostID_Server, RmiContext.ReliableSend, gameCards[Selected]);
                        break;
                    case ConsoleKey.D3:
                        Selected = 2;
                        C2SProxy.PlayCard(HostID.HostID_Server, RmiContext.ReliableSend, gameCards[Selected]);
                        break;
                    case ConsoleKey.D4:
                        Selected = 3;
                        C2SProxy.PlayCard(HostID.HostID_Server, RmiContext.ReliableSend, gameCards[Selected]);
                        break;
                    case ConsoleKey.D5:
                        Selected = 4;
                        C2SProxy.PlayCard(HostID.HostID_Server, RmiContext.ReliableSend, gameCards[Selected]);
                        break;
                    case ConsoleKey.D6:
                        Selected = 5;
                        C2SProxy.PlayCard(HostID.HostID_Server, RmiContext.ReliableSend, gameCards[Selected]);
                        break;
                    case ConsoleKey.D7:
                        Selected = 6;
                        C2SProxy.PlayCard(HostID.HostID_Server, RmiContext.ReliableSend, gameCards[Selected]);
                        break;
                    case ConsoleKey.D8:
                        Selected = 7;
                        C2SProxy.PlayCard(HostID.HostID_Server, RmiContext.ReliableSend, gameCards[Selected]);
                        break;
                    case ConsoleKey.D9:
                        Selected = 8;
                        C2SProxy.PlayCard(HostID.HostID_Server, RmiContext.ReliableSend, gameCards[Selected]);
                        break;
                    case ConsoleKey.D0:
                        Selected = 9;
                        C2SProxy.PlayCard(HostID.HostID_Server, RmiContext.ReliableSend, gameCards[Selected]);
                        break;
                    case ConsoleKey.Spacebar:
                        C2SProxy.DrawCard(HostID.HostID_Server, RmiContext.ReliableSend);
                        break;
                }
            }

            workerThread.Join();
            netClient.Disconnect();
        }
    }
}