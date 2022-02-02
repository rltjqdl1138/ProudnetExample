using Nettention.Proud;

namespace OnecardCommon
{
	public class Vars
	{
		public static System.Guid m_Version = new System.Guid("{ 0x3ae33249, 0xecc6, 0x4980, { 0xbc, 0x5d, 0x7b, 0xa, 0x99, 0x9c, 0x7, 0x39 } }");
		public static int m_serverPort = 33334;

		static Vars()
		{
			
		}

	}

	public class User
	{
		static int UserId = 0;
		public HostID HostId { get; set; }
		public string UserName { get; set; }
		public int UserID { get; set; }
		public int RoomNumber { get; set; }
		public User(string UserName, HostID HostId)
		{
			UserID = ++UserId;
			this.UserName = UserName;
			this.HostId = HostId;
			RoomNumber = 0;
		}
		public User()
		{
			UserName = "Unknown";
			RoomNumber = 0;
		}
	}

	public class GameCard
    {
		public int shape;
		public int number;
		public GameCard()
        {

        }
		public GameCard(int number)
        {
			this.shape = number % 4 + 1;
			this.number = number / 4 + 1;
        }
		public int toNumber()
        {
			return (shape - 1) + (number - 1) * 4;
		}
		public string toString()
        {
			string shape = "";
			string number = "";
            switch (this.shape)
            {
				case 1:
					shape = "♠";
					break;
				case 2:
					shape = "♥";
					break;
				case 3:
					shape = "♣";
					break;
				case 4:
					shape = "◆";
					break;
				default:
					shape = "■";
					break;
            }
            switch (this.number)
            {
				case 0:
					number = " ";
					break;
				case 1:
					number = "A";
					break;
				case 10:
					number = "T";
					break;
				case 11:
					number = "J";
					break;
				case 12:
					number = "Q";
					break;
				case 13:
					number = "K";
					break;
				default:
					number = this.number.ToString();
					break;
			}
			return String.Format("{0}{1}", shape, number);
        }
		public bool Match(GameCard card)
        {
			return this.shape == card.shape || this.number == card.number;
		}
    }

	public class GamePlayer
	{
		public User user;
		public List<GameCard> hand = new List<GameCard>();
		public GamePlayer()
		{

		}
		public GamePlayer(User user)
		{
			this.user = user;
		}
	}
}