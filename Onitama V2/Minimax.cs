using System.Net.Http.Headers;

namespace Onitama
{
	internal class Minimax
	{
		internal static Move BestMove(int depth, Board board, int sign)
		{
			board.previousmoves.Clear(); // accessing the first item should be the move to make from this board, so we need to clear any previous moves.
			// I decided to take advantage of async functions to help this work faster.
			static Board RecursiveHelper(int depth, List<Board> boards, int sign)
			{
				if (boards.Count == 0) // this means there are no more possible moves, which shouldn't happen
				{
					Board b = new Board()
					{
						strength = sign * int.MinValue, // this way it's a bad move, so we can avoid crashing the program
					};
				}
				if (depth == 0) // to save time, given the complexity of this function, we want to stop after a certain number of moves in the future
				{
					boards.First().strength = EvaluateStrength(boards.First());
					Board final = boards.First();
					if (sign == 1) // each player does what's to their own advantage; the starting player is positive and the other is negative
					{
						foreach (Board b in boards)
						{
							b.strength = EvaluateStrength(b);
							if (final.strength < b.strength) final = b;
						}
					}
					else
					{
						foreach (Board b in boards)
						{
							b.strength = EvaluateStrength(b);
							if (final.strength > b.strength) final = b; // end up with the one with the most negative advantage
						}
					}
					return final;
				}
				else
				{
					List<Board> results = new List<Board>();
					foreach (Board b in boards)
					{
						var test = RecursiveHelper(depth - 1, b.PossibleBoards(), -sign);
						results.Add(test);
					}
					// "best" is most negative or most positive number, depending on who's playing
					if (sign == 1) return results.OrderByDescending(b => b.strength).First();
					else return results.OrderBy(b => b.strength).First();
				}
			}
			// first item is the one with the maximum advantage, so pick that one
			var result = RecursiveHelper(depth, board.PossibleBoards(board.previousmove?.piece.color ?? ConsoleColor.Blue), sign); // if previousmove is null, that means the bot went first, and it is always blue
			Move? retval = result.boardchain.First().previousmoves.Find(move => move == result.firstmove);
			if (retval is null) throw new NullReferenceException("Move was null when returning the best move."); // This could become null in multiple ways, and ignoring that is very bad
			return retval;
		}

		internal static bool? EvaluateWin(Board board, out int v)
		{
			ConsoleColor c = board.previousmove?.piece.color ?? Game.cpu.color;
			ConsoleColor othercolor = ConsoleColor.Red; // opponent's color; maybe one day this won't be hardcoded
			static int YInitial(ConsoleColor color) => color == ConsoleColor.Red ? 4 : 0; // red pieces start at index 4 in the array and blue ones at 0
			Piece? othermaster = Piece.GetMaster(board, othercolor);
			Piece? thismaster = Piece.GetMaster(board, c);

			// several game-ending moves to check before evaluating the general cases
			if (othermaster is null) { v = int.MaxValue; return true; } // infinity, basically
			if (othermaster.position == (2, 4)) { v = int.MinValue; return false; }
			if (thismaster is null) { v = int.MinValue; return false; } // minus infinity because we just lost
			if (thismaster.position == (2, 0)) { v = int.MaxValue; return true; }

			int pieceCount = board.pieces.Select(p => p.color == c).Count() - board.pieces.Select(p => p.color == othercolor).Count();
			if (pieceCount != 0) { v = pieceCount * 100; return null; } // arbitrary scalar
															// so that it's always better than a move that just has pieces further along the board (see below)
			var distanceUs = board.pieces.Where(piece => piece.color == c).Select(piece => Math.Abs(piece.position.Item2 - YInitial(piece.color)));
			int elements = distanceUs.Count();
			int thesepieces = distanceUs.Sum();
			int otherpieces = board.pieces.Where(piece => piece.color == othercolor).Select(piece => Math.Abs(piece.position.Item2 - YInitial(piece.color))).Sum();
			// these queries return the sum of the distance from start to current y position of each piece
			// it's negative if the opponent has moved more pieces because that player is assumed to have the advantage, all else being equal
			int startingsquares = board.pieces.Where(piece => piece.position.Item2 != (piece.color == ConsoleColor.Red ? 4 : 0) && piece.color == c).Count();
			// the number of own pieces off of their starting squares; otherwise, the algorithm thinks it's best for one piece to just charge ahead, which is boring.

			v = thesepieces - otherpieces + startingsquares + (elements / 2);
			return null;
			// postcondition: return the relative advantage or disadvantage (v) of the given boardstate for the player who made the move
			// and return true or false iff a player won
		}

		internal static int EvaluateStrength(Board board) // I wrote this first and I'm too lazy to refactor properly
		{
			EvaluateWin(board, out int v); return v;
		}
	}
}