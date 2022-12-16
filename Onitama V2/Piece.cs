namespace Onitama
{
    internal class Piece
    {
        internal bool master;
        internal (int, int) position;
        internal Player player;
        internal ConsoleColor color;
        public Piece((int, int) position, Player player, bool master = false)
        {
            this.master = master;
            this.position = position;
            this.player = player;
            color = player.color;
        }
        internal static Piece? GetMaster(Board board, ConsoleColor color)
        {
            return board.pieces.Find(p => p.master && p.color == color);
        }
        public override string ToString()
        {
            return (color == ConsoleColor.Red ? "R" : "B") + (master ? "M" : "S");
        }
        public static bool operator ==(Piece a, Piece b)
        {
            if (a is null || b is null) return a is null && b is null; // is bypasses the == operator
            return a.position == b.position && a.color == b.color && a.master == b.master;
        }
        public static bool operator !=(Piece a, Piece b) => !(a == b);
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

            try
            {
                return this == (Piece)obj;
            }
            catch { return false; }
        }
    }
}