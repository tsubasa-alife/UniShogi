namespace UniShogi
{
	/// <summary>
	/// 将棋の駒を表すEnum
	/// </summary>
	public enum Piece
	{
		Empty, Pawn, Lance, Knight,
		Silver, Gold, Bishop, Rook,
		King, P_Pawn, P_Lance, P_Knight,
		P_Silver, P_Gold, P_Bishop, P_Rook,
		
		B_None, B_Pawn, B_Lance, B_Knight,
		B_Silver, B_Gold, B_Bishop, B_Rook,
		B_King, B_P_Pawn, B_P_Lance, B_P_Knight,
		B_P_Silver, B_P_Gold, B_P_Bishop, B_P_Rook,

		W_None, W_Pawn, W_Lance, W_Knight,
		W_Silver, W_Gold, W_Bishop, W_Rook,
		W_King, W_P_Pawn, W_P_Lance, W_P_Knight,
		W_P_Silver, W_P_Gold, W_P_Bishop, W_P_Rook,
		
		None

	}

	public static class PieceExtensions
	{
		private static readonly string[] PrettyPiece
			= { "・", "歩", "香", "桂", "銀", "金", "角", "飛", "玉", "と", "杏", "圭", "全", "??", "馬", "竜",
				"??", "v歩", "v香", "v桂", "v銀", "v金", "v角", "v飛", "v玉", "vと", "v杏", "v圭", "v全", "??", "v馬", "v竜" };

		public static string Pretty(this Piece p)
		{
			return PrettyPiece[(int)p];
		}
	}
}