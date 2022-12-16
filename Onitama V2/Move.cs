namespace Onitama
{
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
	internal class Move
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
	{
		internal Card card;
		internal Piece piece;
		internal (int, int) final;
		public Move(Card card, Piece piece, (int, int) position)
		{
			this.card = card;
			this.piece = piece;
			final = position;
		}
		public static bool operator ==(Move a, Move b) { return a.card == b.card && a.piece == b.piece && a.final == b.final; }
		public static bool operator !=(Move a, Move b) { return!(a == b); }
	}
}