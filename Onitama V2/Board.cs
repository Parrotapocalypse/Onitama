using System.Diagnostics;
using System.Reflection;

namespace Onitama
{
	internal class Board
	{
		internal List<Piece> pieces = new List<Piece>();
		internal List<Card> cards = new List<Card>(5);
		internal int strength;
		internal List<Board> boardchain = new List<Board>(); // how we got here so that we know how to get back
		internal Move? previousmove // gets the last move when accessed, or adds the last move to the list when assigned to
		{
			get => previousmoves.LastOrDefault(); // most recent move
			set => previousmoves.Add(value!);
		}
		internal Move? firstmove
		{
			get => previousmoves.Where(m => m.piece.color == ConsoleColor.Blue).FirstOrDefault(); // Blue because the computer is the only thing accessing this property and it's bad for it to see a red move
		}
		internal List<Move> previousmoves = new List<Move>(); // all moves; accessed through the properties above; this would normally be private, but it's a little more convenient to be internal
		public Board(IEnumerable<Piece>? pieces = default)
		{
			if (pieces != null) this.pieces = (List<Piece>)pieces;
			cards = Parser.chosenCards;
		}
		internal Board MakeMove(Move move)
		{
			List<Piece> newpieces = pieces.ConvertAll(p => new Piece(p.position, p.player, p.master)); // I really don't want any side effects from shallow copying, so I construct new pieces that are identical to the old ones
			Piece? found = newpieces.Find(p => p.position == move.piece.position);
			if (move.piece.color == ConsoleColor.Blue)
			{
				if (found is null)
				{
					return this;
				}
				if (move.card.mirror.Any(pos => (Tup)found.position + pos == move.final)) found.position = move.final;
				else
				{
					// reevaluate this card because something got messed up
					found.position = (Tup)found.position - (2, 2) + move.card.mirror.Where(pos => Inside((Tup)pos + found.position - (2, 2))).OrderByDescending(item => Minimax.EvaluateStrength(this)).First();
				}
			}
			else found.position = move.final;
			
			Board b = new Board(newpieces)
			{
				boardchain = boardchain,
			};
			b.boardchain.Add(this);
			b.previousmoves = previousmoves;
			previousmove = move; // add to the list that was copied before

			Parser.chosenCards.Remove(move.card);
			Parser.chosenCards.Insert(2, move.card); // put the card in the middle of the List, meaning that whatever was in the middle is shifted to the correct side, representing how the cards rotate

			// capture a piece, if needed
			b.pieces.RemoveAll(p => p.position == found.position && p.color != found.color);

			return b;
		}
		internal List<Board> PossibleBoards(ConsoleColor? c = null, Board? board = null)
		{
			// this bit below is short for "if board == null, Game.board; else board
			// it's basically an optional parameter but it doesn't have to be a constant
			Board b = board ?? Game.board;
			b.boardchain.Add(this); // aka self in Python; all the boards should know what the board they're "descended" from is
			c ??= previousmove?.piece.color ?? ConsoleColor.Blue; // if all this is null, it means we're dealing with the computer (which is always blue) going first
			List<Board> result = new List<Board>();
			foreach (Piece piece in pieces.Where(p => p.color == c)) // a player can only move their own color of piece, so filter out the opponent's pieces
			{
				foreach (Card card in Parser.chosenCards.ToArray()[3..]) // just the 3rd and 4th elements, representing what the computer has access to
				{
					foreach ((int, int) position in card.mirror)
					{
						// cast the (int, int) to Tup, which is the same thing but allows me to define operator overloads (see Utils.cs)
						//(int, int) dest = piece.position + position - (2, 2); // as regards the math here, moves on the cards are defined with the piece always on (2,2), so we subtract that to get the true final position.
						(int, int) dest = (Tup)piece.position + position - (2, 2);
						// construct an actual Move object
						Move m = new Move(card, piece, dest);
						
						if (Inside(dest) && !b.pieces.Any(p => p.color == piece.color && p.position == dest)) // inside (the board) to mean it's a legal move
						{
							// advance the board with the move and
							// store the updated board (and its corresponding move) in the list (but don't store the same board)
							var add = b.MakeMove(m);
							if (add != b) result.Add(add);
						}
					}
				}
			}
			return result;
			// postcondition: return a List containing the result of every legal Move from the current board
		}
		private static bool Inside((int,int) position)
		{
			bool x = position.Item1 >= 0 && position.Item1 <= 4;
			bool y = position.Item2 >= 0 && position.Item2 <= 4;
			return x && y;
		}
		internal void Print()
		{
			// prints the board to the console, separating horizontally with pipes
			for (int y = 0; y < 5; y++)
			{
				for (int x = 0; x < 5; x++)
				{
					if (x == 0) Parser.Log(Utils.Alphabet(y).ToString()); // convert the number to a letter
					Piece? piece = pieces.Find(p => p.position == (x, y)); // find the piece at that position, or null if there isn't one
					if (piece is null)
					{
						if (x == 2 && y == 0) Parser.Log("|$$", ConsoleColor.Blue); // starting square -- you win if your master lands here!
						else if (x == 2 && y == 4) Parser.Log("|$$", ConsoleColor.Red);
						else Parser.Log("|  "); // if null, fill that square with spaces
					}
					else Parser.Log("|" + piece.ToString(), piece.color); // or print the piece
					if (x == 4) Parser.LogLine("|"); // add another pipe when you reach the end of the line
				}
			}
			Parser.LogLine(""); // add an extra newline for more separation between moves and such
		}
		public static bool operator ==(Board a, Board b)
		{
			return a.pieces == b.pieces && a.cards == b.cards;
		}
		public static bool operator !=(Board a, Board b) => !(a == b);
	}
}