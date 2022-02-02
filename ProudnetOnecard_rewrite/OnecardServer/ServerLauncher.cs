using OnecardCommon;
using Nettention.Proud;
using System.Collections.Concurrent;

namespace OnecardServer
{
    public class ServerLauncher
    {
        public bool RunLoop;
        public static readonly NetServer NetServer = new NetServer();
        private readonly Nettention.Proud.ThreadPool _netWorkerThreadPool = new Nettention.Proud.ThreadPool(8);
        private readonly Nettention.Proud.ThreadPool _userWorkerThreadPool = new Nettention.Proud.ThreadPool(8);
        public static ConcurrentDictionary<HostID, User> UserList { get; } = new ConcurrentDictionary<HostID, User>();

        static public GameRoom[] RoomArray = new GameRoom[10];
        Handler Handler = new Handler();
        process.CommonProcess Process = new process.CommonProcess();
        process.GameRoomProcess gameRoomProcess = new process.GameRoomProcess();

        public void InitializeStub()
        {
            Process.InitStub();
            gameRoomProcess.InitStub();
        }
        public void InitializeHandler()
        {
            NetServer.ConnectionRequestHandler = Handler.ConnectionRequestHandler;
            NetServer.ClientHackSuspectedHandler = Handler.ClientHackSuspectedHandler;
            NetServer.ClientJoinHandler = Handler.ClientJoinHandler;
            NetServer.ClientLeaveHandler = Handler.ClientLeaveHandler;
            NetServer.ErrorHandler = Handler.ErrorHandler;
            NetServer.WarningHandler = Handler.WarningHandler;
            NetServer.ExceptionHandler = Handler.ExceptionHandler;
            NetServer.InformationHandler = Handler.InformationHandler;
            NetServer.NoRmiProcessedHandler = Handler.NoRmiProcessedHandler;
            NetServer.P2PGroupJoinMemberAckCompleteHandler = Handler.P2PGroupJoinMemberAckCompleteHandler;
            NetServer.TickHandler = Handler.TickHandler;
            NetServer.UserWorkerThreadBeginHandler = Handler.UserWorkerThreadBeginHandler;
            NetServer.UserWorkerThreadEndHandler = Handler.UserWorkerThreadEndHandler;
        }
        public void InitialzieServerParameter()
        {
            var parameter = new StartServerParameter();
            parameter.protocolVersion = new Nettention.Proud.Guid(Vars.m_Version);
            parameter.tcpPorts.Add(Vars.m_serverPort);
            NetServer.Start(parameter);

            for (int i = 0; i < RoomArray.Length; i++)
                RoomArray[i] = new GameRoom();
              
        }
        public void ServerStart()
        {
            InitializeHandler();
            InitializeStub();
            InitialzieServerParameter();
            RunLoop = true;

            GameRoom tempRoom = new GameRoom();
            tempRoom.EnterPlayer(new User());
            tempRoom.EnterPlayer(new User());
            tempRoom.EnterPlayer(new User());

            tempRoom.InitializeGame();
            for (int i = 0; i < tempRoom.players.Count; i++) {
                String str = String.Format("Player{0} :", i);
                for (int j = 0; j < tempRoom.players[i].hand.Count; j++)
                    str += tempRoom.players[i].hand[j].toString() + "  ";
                Console.WriteLine(str);
            }
            Console.WriteLine("First Card: " + tempRoom.lastCard.toString());
            string deck = "Unused:";
            while (tempRoom.unusedDeck.Count > 0)
                deck += tempRoom.unusedDeck.Pop().toString() + "  ";
            Console.WriteLine(deck);
        }
        public void Dispose()
        {
            NetServer.Dispose();
        }
    }
}
