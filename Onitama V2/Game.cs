using System.Diagnostics;

namespace Onitama
{
	internal class Game
	{
		internal static Player human = new Player(ConsoleColor.Red);
		internal static Player cpu = new Player(ConsoleColor.Blue);
		internal static Board board = new Board();
		internal static void StartGame()
		{
			for (int i = 0; i < 5; i++)
			{
									// the third piece in the row is the master
				board.pieces.Add(new Piece((i, 0), cpu, i == 2));
				board.pieces.Add(new Piece((i, 4), human, i == 2));
			}
			// starting player is chosen by the color of the card that's on the side
			// begin the game with the turn of whoever goes first
			board.Print();
			if (HumanFirst()) HumanTurn();
			else ComputerTurn();
		}
		internal static bool HumanFirst() => Parser.chosenCards.Last().color == ConsoleColor.Red;
		private static void HumanTurn()
		{
			Parser.chosenCards[0].Print();
			Parser.chosenCards[1].Print();
			board = board.MakeMove(Parser.Parse());
			board.Print();
			Piece? othermaster = Piece.GetMaster(board, ConsoleColor.Red);
			Piece? thismaster = Piece.GetMaster(board, ConsoleColor.Blue);
			bool? win = null;
			// something inelegant has to happen with this because I don't want to call position on a null object, which happens without the is not null check.
			if (othermaster is null || (thismaster is not null && thismaster.position == (2, 4))) win = true;
			if (thismaster is null || othermaster!.position == (2, 0)) win = false;
			if (win != null)
			{
				if ((bool)win)
				{
					Parser.LogLine("You win!", ConsoleColor.Green);
				}
				else
				{
					Parser.LogLine("You lose...", ConsoleColor.DarkYellow);
				}
				Parser.LogLine("Thanks for playing!");
				Task.Delay(-1); // wait forever because we're done
			}
			else ComputerTurn();
		}
		private static void ComputerTurn()
		{
			int sign = HumanFirst() ? -1 : 1;
			board = board.MakeMove(Minimax.BestMove(5, board, sign)); // the complexity is terrible!
			Parser.LogLine("Your opponent moved to " + Utils.Alphabet(board.previousmove!.final.Item2)+ (board.previousmove!.final.Item1+1) + " using " + board.previousmove!.card.name + ".");
			board.Print();
			// same as above but flipped around because it's the computer's turn
			Piece? othermaster = Piece.GetMaster(board, ConsoleColor.Red);
			Piece? thismaster = Piece.GetMaster(board, ConsoleColor.Blue);
			bool? win = null;
			if (othermaster is null || (thismaster is not null && thismaster.position == (2, 4))) win = true;
			if (thismaster is null || othermaster!.position == (2, 0)) win = false;
			if (win != null)
			{
				if ((bool)win)
                {
                    Parser.LogLine("You win!", ConsoleColor.Green);
                }
                else
                {
                    Parser.LogLine("You lose...", ConsoleColor.DarkYellow);
                }
                Parser.LogLine("Thanks for playing!");
                Task.Delay(-1);
            }
			HumanTurn();
		}
	}
}
