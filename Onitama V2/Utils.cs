namespace Onitama
{
	public static class Extensions
	{
		// this class has intermittently been in use; I use it for extension methods.
	}
	internal static class Utils
	{
		internal static ConsoleColor Not(ConsoleColor c)
		{
			// precondition: c is Red or Blue
			return c == ConsoleColor.Red ? ConsoleColor.Blue : c == ConsoleColor.Blue ? ConsoleColor.Red : throw new ArgumentOutOfRangeException(nameof(c), nameof(c) + " is not Red or Blue.");
		}
		private static readonly char[] alphabet = "abcde".ToCharArray();
		internal static int Alphabet(char c, bool add = false)
		{
			int i = Array.IndexOf(alphabet, char.ToLower(c));
			if (i != -1) return i + (add ? 1 : 0); // -1 is default value if not present; add 1 if displaying to the console because humans and how they count
			else throw new ArgumentOutOfRangeException(); // precondition: c is in [abcde]
		}
		internal static char Alphabet(int i)
		{
			// precondition: i is an index of alphabet
			if (i >= 0 && i <= alphabet.Length) return alphabet[i];
			else throw new ArgumentOutOfRangeException();
		}

		internal static Tup Mirror(Tup move) // the computer sees the cards in the opposite orientation from the player.
		{
			move -= (2, 2);
			int a = 2 - move.a;
			int b = 2 - move.b;
			return new Tup(a, b);
			// return a Tup reflected on both the x and y axes, using (2,2) as the origin
		}
	}
	internal class Tup // Tup being short for Tuple; this one stores ints specifically.
	/*
	 * What's going on with this class?
	 * Well, C# is cool. You can overload operators for custom classes, like I do here for +, -, ==, and !=
	 * You can add new methods to existing classes.
	 * Sadly, what you can't do is overload operators for existing classes.
	 * For the coordinates of pieces, I've been using (int, int), which is basically a tuple. I want to do math with them,
	 * but they don't have any operators defined for them.
	 * Thus, I defined Tup, which does the same thing, but it's annoying to type out.
	 * So, I define implicit casts between Tup and (int, int), meaning the compiler can cast it without me telling it to do so,
	 * since no information is lost.
	 * If you see code where one (int, int) is cast to a Tup, this tells the compiler to cast the other one, do the operation,
	 * and then cast the resulting value to an (int, int). Exciting!
	 */
	{
		internal int a;
		internal int b;
		public Tup (int a, int b)
		{
			this.a = a;
			this.b = b;
		}
		// Objects normally use reference equality, but really want value equality because I'm doing math with these
		public static bool operator ==(Tup first, Tup second) => first.a == second.a && first.b == second.b;
		public static bool operator !=(Tup a, Tup b) => !(a == b);
		// The first two define casts between the Tup and (int, int) types
		public static implicit operator (int,int)(Tup tup) { return (tup.a, tup.b); }
		public static implicit operator Tup((int a, int b) tuple) { return new Tup(tuple.a, tuple.b); }
		// These define the + and - operations between Tups
		public static Tup operator +(Tup a, Tup b) { return new Tup(a.a + b.a, a.b + b.b); }
		public static Tup operator -(Tup a, Tup b) { return new Tup(a.a - b.a, a.b - b.b); }
		// This is honestly basically the same as the Fraction class everyone uses as an example for custom types when introducing OOP

		public override string ToString()
		{
			return "(" + a.ToString() + ", " + b.ToString() + ")";
		}

		// it's expected to override these when overriding == and !=, even though I don't use them
		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (ReferenceEquals(obj, null))
			{
				return false;
			}
			return this == (Tup)obj;
		}
		public override int GetHashCode()
		{
			return (int)Math.Pow(a.GetHashCode(), b.GetHashCode());
		}
		
	}
}
