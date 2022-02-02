
using Nettention.Proud;
using OnecardCommon;
namespace OnecardServer
{
	public class GameRoom
	{
		public int status;
		public int turn;

		public List<GamePlayer> players = new List<GamePlayer>();

		public Stack<GameCard> usedDeck = new Stack<GameCard>();
		public Stack<GameCard> unusedDeck = new Stack<GameCard>();
		public GameCard lastCard = new GameCard();

		public bool EnterPlayer(User user)
		{
			int numPlayer = players.Count();
			if (numPlayer >= 4) return false;

			GamePlayer newPlayer = new GamePlayer(user);

			players.Add(newPlayer);
			return true;
		}
		public bool InitializeGame()
		{
			if (status == 1 || players.Count < 2)
				return false;
			status = 1;
			Console.WriteLine("Game Start!");

			SetDeck();

			SplitCard();

			return true;
		}

		private void SetDeck()
		{
			GameCard[] deck = new GameCard[53];
			Random random = new Random();

			// 배열에 카드를 랜덤으로 생성합니다.
			for (int i = 0; i < deck.Length - 1;)
			{
				int rand = random.Next(0, 52);
				if (deck[rand] == null) deck[rand] = new GameCard(i++);
			}

			// usedDeck에 카드를 추가합니다.
			for (int i = 0; i < deck.Length - 1; i++)
				usedDeck.Push(deck[i]);

			// usedDeck에 있는 카드를 섞어 unusedDeck으로 옮깁니다.
			MergeDeck();
		}

		private void MergeDeck()
		{
			GameCard[] deck = new GameCard[53];
			Random random = new Random();

			while (usedDeck.Count() > 0)
			{
				int rand = random.Next(0, 52);
				if (deck[rand] == null)
					deck[rand] = usedDeck.Pop();
			}
			for (int i = 0; i < deck.Length; i++)
			{
				if (deck[i] == null) continue;
				unusedDeck.Push(deck[i]);
			}
		}

		private void SplitCard()
		{
			for (int j = 0; j < 7; j++)
				for (int i = 0; i < players.Count(); i++)
					players[i].hand.Add(unusedDeck.Pop());

			usedDeck.Push(unusedDeck.Pop());
			lastCard = usedDeck.Peek();
		}
		public void EndGame()
		{
			status = 0;
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
			// 핸드 개수 확인
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

		public GameCard getLastCard()
		{
			return usedDeck.Peek();
		}

	}
}
