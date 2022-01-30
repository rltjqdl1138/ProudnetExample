using Nettention.Proud;
using OnecardCommon;

namespace OnecardServer.process
{
    public class CommonProcess
    {
        static internal S2C.Proxy S2CProxy = new S2C.Proxy();
        static internal C2S.Stub C2SStub = new C2S.Stub();

        public void InitStub()
        {
            C2SStub.Login = Login;
            C2SStub.EnterRoom = EnterRoom;
            C2SStub.LeaveRoom = LeaveRoom;


            ServerLauncher.NetServer.AttachProxy(S2CProxy);
            ServerLauncher.NetServer.AttachStub(C2SStub);
        }
        static public bool Login(HostID remote, RmiContext rmiContext, string UserName)
        {
            string message = string.Format("{0} entered.", UserName);
            User user = new User(UserName, remote);

            ServerLauncher.UserList.TryAdd(remote, user);

            S2CProxy.ResponseLogin(user.HostId, rmiContext, user);

            Console.WriteLine(message);
            return true;
        }

        // * 추가 * //
        static public bool EnterRoom(HostID remote, RmiContext rmiContext, int RoomNumber)
        {
            GameRoom room = ServerLauncher.RoomArray[RoomNumber];
            ServerLauncher.UserList.TryGetValue(remote, out User user);
            user.RoomNumber = RoomNumber;
            bool isSuccess = room.EnterPlayer(user);

            if (!isSuccess)
            {
                S2CProxy.ResponseEnter(remote, rmiContext, 0, 0);
                return true;
            }

            ServerLauncher.UserList[remote] = user;

            string message = string.Format("{0} entered to Room {1}", user.UserName, user.RoomNumber);

            // Notify To Room 

            S2CProxy.ResponseEnter(remote, rmiContext, RoomNumber, room.players.Count() - 1);
            Console.WriteLine(message);
            return true;
        }
        static public bool LeaveRoom(HostID remote, RmiContext rmiContext)
        {
            return true;
        }

    }
}