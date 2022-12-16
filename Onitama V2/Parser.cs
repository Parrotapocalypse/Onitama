using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Onitama
{
	internal static class Parser
	{
		internal static Dictionary<string, Card> cardsByName = new Dictionary<string, Card>();
		/*
		the below regex matches commands like the following:
		a5 b5
		a5 to b5
		a5 > b5
		A5 B5
		and so on
		*/
		readonly static Regex move = new Regex(@"[a-e][1-5] ((>|(to)) ){0,1}[a-e][1-5]", RegexOptions.IgnoreCase);
		internal static List<Card> chosenCards = new List<Card>(5);
		private static readonly string slash = Environment.CurrentDirectory.Contains('\\') ? "\\" : "/"; // depends on running Windows or not
		private static bool openedFile = false;
		static Parser() // static constructor called when class is accessed the first time
		{
			StreamReader GetStreamReader(string location = "") // local function
			{
				try
				{
					if (location != "") return new StreamReader(location);
					else return new StreamReader(Console.ReadLine()!);
				}
				catch
				{
					// I actually can't use my Log() methods because we're inside the constructor, so I use these instead
					Console.WriteLine("The program was unable to locate CardData.txt.");
					Console.Write("Please input the full path to your CardData.txt file: ");
					return GetStreamReader("");
				}
			}
			StreamReader streamReader = GetStreamReader(Environment.CurrentDirectory + slash + "CardData.txt");
			foreach (string str in streamReader.ReadToEnd().Split("\n"))
			{
				string[] s = str.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				if (Enum.TryParse(typeof(ConsoleColor), s[2], ignoreCase: true, out object? c) && c != null) cardsByName.Add(s[0], new Card(s[0], s[1], (ConsoleColor)c));
				// because the conversion of string to ConsoleColor could fail, I use TryParse() and then I can cast it to ConsoleColor
			}
			Random random = new Random();
			// ain't C# wonderful? Here I take a string of cards, split into an array, then "sort" by random numbers
			// and take the first 5 entries, which is how many the game uses at one time.
			chosenCards.AddRange(cardsByName.Values.OrderBy(_ => random.NextDouble()).Take(5));
		}

		internal static void Log(string message, ConsoleColor color = ConsoleColor.White)
		{
			// I want a convenient way to change the color for only a particular block of text
			ConsoleColor previous = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.Write(message);
			Console.ForegroundColor = previous;
		}
		internal static void LogLine(string message, ConsoleColor color = ConsoleColor.White)
		{
			// why rewrite the same code when you could just do this?
			Log(message, color);
			Console.WriteLine();
		}
		internal static Move Parse()
		{
			Log("Make a move: ");
			string? s = Console.ReadLine();
			while (s == null)
			{
				LogLine("Message was null. Please try again.", ConsoleColor.DarkRed);
				s = Console.ReadLine(); // try again
			}
			return Parse(s);
		}
		internal static Move Parse(string s)
		{
			// precondition: s is not empty
			// there's a lot of things that could go wrong and the answer is always "return Parse()"
			if (s.Trim() == "") return Parse(); // try to read from the console
			if (s.ToLower().StartsWith("cards") || s.ToLower().StartsWith("help"))
			{
				chosenCards.ForEach(card => card.Print(true));
				if (openedFile == false)
				{
					try
					{
						System.Diagnostics.Process.Start(Environment.CurrentDirectory + slash + "README.md");
						openedFile = true;
					}
					catch { LogLine("Tried to open README.md and couldn't find it. You could just do it yourself, though.\n"+ Environment.CurrentDirectory + slash + "README.md", ConsoleColor.Yellow); }
				}
				return Parse();
			}
			Match search = move.Match(s);
			if (search.Success)
			{
				string match = search.Value.Trim(); // the command itself, if embedded in a larger string
				(int, int) dest;
				int y1 = default;
				try
				{
					// there's a bit of weirdness since the parser reads (y,x) but I then convert to (x,y)
					y1 = Utils.Alphabet(match[0]);
					dest.Item2 = Utils.Alphabet(match[^2]); // ^ is the equivalent of a negative index, by the way
				}
				catch
				{
					LogLine("Your letters don't seem to match coordinates on the board. Please try again.");
					return Parse();
				}
				if (int.TryParse(match[1].ToString(), out int x1)
					&& int.TryParse(match[^1].ToString(), out dest.Item1))
				// Each TryParse() returns a bool and assigns to the specified out parameter
				{
					dest.Item1 -= 1;
					Piece? piece = Game.board.pieces.Find(x => x.color == ConsoleColor.Red && x.position == (x1-1, y1)); // The player can only move red pieces
					if (piece is null)
					{
						LogLine("Couldn't find the piece you want to move. Try again?", ConsoleColor.Yellow);
						return Parse();
					}
					// the next thing to do is to figure out what card the player meant to use. If they could have used either, we have to ask.
					Card card;
					List<Card> cards = new List<Card>();
					for (int i = 0; i < 2; i++)
					{
						foreach ((int, int) delta in chosenCards[i].positions)
						{
							if ((Tup)delta - (2, 2) == (Tup)dest-piece.position) cards.Add(chosenCards[i]);
						}
					}
					if (cards.Count == 0)
					{
						LogLine("There are no cards that allow you to make that move. Please try a different move.", ConsoleColor.Yellow);
						return Parse();
					}
					else if (cards.Count == 1) card = cards.First();
					else
					{
						LogLine("Please choose a card to make this move with. You may also cancel your move by typing \"Cancel.\"");
						string input = Console.ReadLine() ?? ""; // if ReadLine() is null, set input to empty string
						while (!cards.Any(card => card.name == input.ToLower()))
						{
							LogLine("Please choose a card to make this move with. You may also cancel your move by typing \"Cancel.\"", ConsoleColor.Yellow);
							LogLine("Choices:");
							cards.ForEach(card => card.Print());
							input = Console.ReadLine() ?? "";
							if (input.ToLower().StartsWith("cancel")) return Parse(); // start over
						}
						card = cards.Find(card => card.name == input)!;
					}
					if (card is null || piece is null) throw new NullReferenceException(); // postcondition: return Move object composed of a card and piece specified by the user
					return new Move(card, piece, dest);
					// subtract 1 from the y coordinate because the human counts from 1, but computers count from 0 (Alphabet counts from 0, so the x coordinate is fine)
				}
				else
				{
					LogLine("Invalid move. Check the README or Parser.cs for possible moves.");
					return Parse();
				}
			}
			else // search.Success takes care of an exception, but we still need to try again (and tell the user so)
			{
				LogLine("Failed to read your move. Please try again. Check README.txt for how to format your move.\nRegex to match: [a-e][1-5] ((>|(to)) ){0,1}[a-e][1-5]");
				return Parse();
			}
		}
	}
}
// Oh and I didn't see a good place to mention this, but in Parse(), the TryParse methods have match[].ToString() because of a weird type thing when using indexing on strings (it's readonly, but not the keyword)