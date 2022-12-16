namespace Onitama
{
	internal class Card
	{
		internal ConsoleColor color;
		internal string name;
		internal List<(int, int)> positions = new List<(int, int)>();
		internal List<(int, int)> mirror { get => positions.ConvertAll(pos => ((int, int))Utils.Mirror(pos)); }
		internal Card(string name, string xylist, ConsoleColor color)
		{
			this.name = name;
			this.color = color;
			foreach (string move in xylist.Split(' '))
			{
				// extract the coordinates from the string as ints
				if (int.TryParse(move[0].ToString(), out int x) && int.TryParse(move[1].ToString(), out int y)) positions.Add((x, y));
				else Parser.LogLine("[Warning] Failed to parse " + move + " from card " + name, ConsoleColor.Yellow);
			}
		}
		internal void Print(bool full = false)
		{
			if (full) // make it more compact, mainly, since there's only so much space on the screen
			{
                Parser.LogLine(name + ":", color);
				int index = Parser.chosenCards.FindIndex(c => c.name == name);
				if (index != -1) // meaning failed to find
				{
					// 0, 1 is in your hand; 2 is in the middle and because you always do this command after your opponent has moved, that card is coming to you.
					// 3, 4 is in the opponent's hand.
					Parser.LogLine(index == 2 ? "(in the middle; you will receive this next turn)" : index > 2 ? "(available to your opponent)" : "(available to you)");
				}
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (i == 2 && j == 2) Parser.Log("|@"); // the piece's starting location
                        else if (positions.Contains((j, i))) Parser.Log("|#", color); // can move here
                        else Parser.Log("| "); // can't move here
                        if (j == 4) Parser.LogLine("|");
                    }
                }
            }
			else
			{
				Parser.LogLine(name + ":", color);
				for (int i = 0; i < 5; i++)
				{
					for (int j = 0; j < 5; j++)
					{
						if (i == 2 && j == 2) Parser.Log("| @ "); // the piece's starting location
						else if (positions.Contains((j, i))) Parser.Log("| # ", color); // can move here
						else Parser.Log("|   "); // can't move here
						if (j == 4) Parser.LogLine("|");
					}
				}
				// postcondition: return a 5x5 grid highlighting the squares a piece can move to
				// this way of doing it partially resembles the cards on paper
			}
		}
		public static bool operator ==(Card a, Card b) => a.name == b.name;
		public static bool operator !=(Card a, Card b) => !(a == b);
	}
}