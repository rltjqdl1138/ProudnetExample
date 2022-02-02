using Nettention.Proud;
using OnecardCommon;

namespace OnecardServer.process
{
    public class GameRoomProcess : CommonProcess
    {
        public new void InitStub()
        {
            C2SStub.Start = Start;
            C2SStub.PlayCard = PlayCard;
            C2SStub.DrawCard = DrawCard;
            C2SStub.ChangeShape = ChangeShape;
        }
        static public bool Start(HostID remote, RmiContext rmiContext)
        {
            ServerLauncher.UserList.TryGetValue(remote, out User user);
            int RoomNumber = user.RoomNumber;
            if (RoomNumber == 0) return true;

            GameRoom room = ServerLauncher.RoomArray[RoomNumber];

            bool isSuccess = room.InitializeGame();
            if (!isSuccess)
                return true;

            S2CProxy.NotifyStartGame(room.GetHostIDs(), RmiContext.ReliableSend, 0, room.lastCard);
            for (int i = 0; i < room.players.Count; i++)
            {
                GamePlayer player = room.players[i];
                S2CProxy.ResponseDraw(player.user.HostId, rmiContext, player.hand);
            }
            return true;
        }
        static public bool PlayCard(HostID remote, RmiContext rmiContext, GameCard card)
        {
            ServerLauncher.UserList.TryGetValue(remote, out User user);
            int RoomNumber = user.RoomNumber;
            if (RoomNumber == 0)
                return true;

            GameRoom room = ServerLauncher.RoomArray[RoomNumber];
            int playerID = room.GetPlayerID(user);

            if (playerID != room.turn)
                return true;
            
            GamePlayer player = room.players[playerID];
            int countHand = player.hand.Count;
            int hand = player.hand.FindIndex(p => p.toNumber() == card.toNumber());
            bool isSuccess = room.PlayHand(hand);
            if (!isSuccess)
                return true;

            var hosts = room.GetHostIDs();
            S2CProxy.ChangeLastCard(hosts, rmiContext, room.getLastCard());
            S2CProxy.ChangeHand(hosts, rmiContext, playerID, player.hand.Count());

            if (countHand == 1)
            {
                room.EndGame();
                S2CProxy.NotifyEndGame(hosts, rmiContext, playerID);
            }
            else
            {
                room.setNextTurn();
                S2CProxy.ChangeTurn(hosts, rmiContext, room.turn);
            }
            
            return true;
        }
        static public bool DrawCard(HostID remote, RmiContext rmiContext)
        {
            ServerLauncher.UserList.TryGetValue(remote, out User user);
            int RoomNumber = user.RoomNumber;
            if (RoomNumber == 0)
                return true;

            GameRoom room = ServerLauncher.RoomArray[RoomNumber];
            int playerID = room.GetPlayerID(user);

            if (playerID != room.turn)
                return true;

            GamePlayer player = room.players[playerID];
            int count = room.DrawHand();
            if (count > 0)
            {
                var hosts = room.GetHostIDs();
                S2CProxy.ChangeHand(hosts, rmiContext, playerID, player.hand.Count());
                S2CProxy.ResponseDraw(player.user.HostId, rmiContext, player.hand);
                room.setNextTurn();
                S2CProxy.ChangeTurn(hosts, rmiContext, room.turn);
            }

            return true;
        }
        static public bool ChangeShape(HostID remote, RmiContext rmiContext, int shape)
        {
            return true;
        }
    }
}
