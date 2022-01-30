using Nettention.Proud;

namespace OnecardCommon
{
    public class Marshaler : Nettention.Proud.Marshaler
    {
        public static void Write(Message msg, GameCard card)
        {
            int num = (card.shape - 1) + (card.number - 1) * 4;
            msg.Write(num);
            /*
             * msg.Write(card.shape);
             * msg.Write(card.number);
             */
        }
        public static GameCard Read(Message msg, out GameCard card)
        {
            msg.Read(out int num);
            /*
            msg.Read(out int shape);
            msg.Read(out int number);
            */
            
            card = new GameCard(num);

            return card;
        }

        public static void Write(Message msg, List<GameCard> cards)
        {

            if (cards == null)
                msg.Write(0);
            else
            {
                msg.Write(cards.Count);
                foreach (GameCard card in cards)
                    Write(msg, card);
            }
        }
        public static List<GameCard> Read(Message msg, out List<GameCard> cards)
        {
            cards = new List<GameCard>();
            msg.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                msg.Read(out int num);
                cards.Add(new GameCard(num));
            }
            return cards;
        }

        public static void Write(Message msg, User user)
        {
            msg.Write(user.HostId);
            msg.Write(user.UserName);
            msg.Write(user.UserID);
        }
        public static User Read(Message msg, out User user)
        {
            msg.Read(out HostID HostId);
            msg.Read(out string UserName);
            msg.Read(out int UserID);

            user = new User();
            user.HostId = HostId;
            user.UserName = UserName;
            user.UserID = UserID;

            return user;
        }
    }
}
