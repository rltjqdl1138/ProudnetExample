
using Nettention.Proud;
using OnecardCommon;
namespace OnecardServer
{
	public class GameRoom
	{
		public int id;
		public int status;
		public int turn;

		public List<GamePlayer> players = new List<GamePlayer>();

		public GameCard lastCard = new GameCard();
		public Stack<GameCard> usedDeck = new Stack<GameCard>();
		public Stack<GameCard> unusedDeck = new Stack<GameCard>();

		public bool InitializeGame()
        {
			if (status == 1 || players.Count < 2)
				return false;
			Console.WriteLine("Game Start!");
			status = 1;

			setDeck();

			SplitCard();
			
			return true;
		}
		public HostID[] GetHostIDs()
        {
			HostID[] users = players
				.Select(p => p.user.HostId)
				.ToArray();
			return users;
		}
		public int GetPlayerID(User user)
        {
			GameRoom room = ServerLauncher.RoomArray[user.RoomNumber];
			return room.players.FindIndex(p => p.user.HostId == user.HostId);
		}
		private void setDeck()
		{
			GameCard[] deck = new GameCard[53];
			Random random = new Random();

			for (int i = 0; i < deck.Length - 1;)
			{
				int rand = random.Next(0, 52);
				if (deck[rand] == null) deck[rand] = new GameCard(i++);
			}

			for (int i = 0; i < deck.Length - 1; i++)
				unusedDeck.Push(deck[i]);
		}

		private void SplitCard()
		{
			for (int j = 0; j < 7; j++)
				for (int i = 0; i < players.Count(); i++)
					players[i].hand.Add(unusedDeck.Pop());

			usedDeck.Push(unusedDeck.Pop());
		}

		public GameCard getLastCard()
		{
			return usedDeck.Peek();
		}
		public void setNextTurn()
		{
			if (++turn == players.Count())
				turn = 0;
		}
		public int DrawHand()
        {
			GameCard gameCard = unusedDeck.Pop();
			players[turn].hand.Add(gameCard);
			return 1;
		}
        public bool PlayHand(int hand)
		{
			int currentHand = players[turn].hand.Count();
			if (hand < 0 || hand >= currentHand)
				return false;

			GameCard gameCard = players[turn].hand[hand];

			bool isMatched = getLastCard().Match(gameCard);
			if (isMatched)
			{
				usedDeck.Push(gameCard);
				players[turn].hand.RemoveAt(hand);

				return true;
			}
			return false;

		}
		public bool EnterPlayer(User user)
        {
			int numPlayer = players.Count();
			if (numPlayer >= 4) return false;

			GamePlayer newPlayer = new GamePlayer(user);

			players.Add(newPlayer);
			return true;
		}

	}
}
