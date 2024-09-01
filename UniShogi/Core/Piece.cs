/*
 * Copyright (c) 2022 tomori-k
 */

namespace UniShogi
{
	/// <summary>
	/// 将棋の駒を表すEnum
	/// </summary>
	public enum Piece
	{
		Empty = 0, Pawn, Lance, Knight,
		Silver, Gold, Bishop, Rook,
		King, P_Pawn, P_Lance, P_Knight,
		P_Silver, P_Gold, P_Bishop, P_Rook,
		
		B_Empty = 0, B_Pawn, B_Lance, B_Knight,
		B_Silver, B_Gold, B_Bishop, B_Rook,
		B_King, B_P_Pawn, B_P_Lance, B_P_Knight,
		B_P_Silver, B_P_Gold, B_P_Bishop, B_P_Rook,

		W_Empty, W_Pawn, W_Lance, W_Knight,
		W_Silver, W_Gold, W_Bishop, W_Rook,
		W_King, W_P_Pawn, W_P_Lance, W_P_Knight,
		W_P_Silver, W_P_Gold, W_P_Bishop, W_P_Rook,
		
		None,
		
		KindMask = 0b00111,
		ColorlessMask = 0b01111,
		DemotionMask = 0b10111,
		ColorBit = 0b10000,
		PromotionBit = 0b01000,

	}

	/// <summary>
	/// 将棋の駒に関する拡張メソッド
	/// </summary>
	public static class PieceExtensions
	{
		public static Color Color(this Piece p)
		{
			return (Color)((uint)p >> 4);
		}
		
		/// <summary>
		/// p の成ビットを立てた Piece を作成
		/// </summary>
		public static Piece Promoted(this Piece p)
		{
			return p | Piece.PromotionBit;
		}

		/// <summary>
		/// p の成ビットを下ろした Piece を作成
		/// </summary>
		public static Piece Demoted(this Piece p)
		{
			return p & Piece.DemotionMask;
		}

		/// <summary>
		/// p を c の駒に変換した Piece を作成
		/// </summary>
		public static Piece Colored(this Piece p, Color c)
		{
			return (p & Piece.ColorlessMask) | (Piece)((uint)c << 4);
		}

		/// <summary>
		/// p の Color ビット、成ビットを下ろした Piece を作成
		/// </summary>
		public static Piece Kind(this Piece p)
		{
			return p & Piece.KindMask;
		}

		/// <summary>
		/// p の Color ビットを下ろした Piece を作成
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static Piece Colorless(this Piece p)
		{
			return p & Piece.ColorlessMask;
		}

		/// <summary>
		/// p が成り駒か判定
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static bool IsPromoted(this Piece p)
		{
			return (p & Piece.PromotionBit) != Piece.Empty
			       && p.Colorless() != Piece.King;
		}
		
		public static readonly Piece[] PawnToRook
			= { Piece.Pawn, Piece.Lance, Piece.Knight, Piece.Silver, Piece.Gold, Piece.Bishop, Piece.Rook };
		
		private static readonly string[] PrettyPiece
			= { "・", "歩", "香", "桂", "銀", "金", "角", "飛", "玉", "と", "杏", "圭", "全", "??", "馬", "竜",
				"??", "v歩", "v香", "v桂", "v銀", "v金", "v角", "v飛", "v玉", "vと", "v杏", "v圭", "v全", "??", "v馬", "v竜" };

		private static readonly Dictionary<Piece, char> PieceToChar
            = new Dictionary<Piece, char> {
                { Piece.B_Pawn  , 'P' },
                { Piece.B_Lance , 'L' },
                { Piece.B_Knight, 'N' },
                { Piece.B_Silver, 'S' },
                { Piece.B_Gold  , 'G' },
                { Piece.B_Bishop, 'B' },
                { Piece.B_Rook  , 'R' },
                { Piece.B_King  , 'K' },
                { Piece.W_Pawn  , 'p' },
                { Piece.W_Lance , 'l' },
                { Piece.W_Knight, 'n' },
                { Piece.W_Silver, 's' },
                { Piece.W_Gold  , 'g' },
                { Piece.W_Bishop, 'b' },
                { Piece.W_Rook  , 'r' },
                { Piece.W_King  , 'k' },
            };

        private static readonly Dictionary<Piece, string> PieceToCsaNoColor = new Dictionary<Piece, string>
        {
            { Piece.Pawn     , "FU" },
            { Piece.Lance    , "KY" },
            { Piece.Knight   , "KE" },
            { Piece.Silver   , "GI" },
            { Piece.Gold     , "KI" },
            { Piece.Bishop   , "KA" },
            { Piece.Rook     , "HI" },
            { Piece.King     , "OU" },
            { Piece.P_Pawn  , "TO" },
            { Piece.P_Lance , "NY" },
            { Piece.P_Knight, "NK" },
            { Piece.P_Silver, "NG" },
            { Piece.P_Bishop, "UM" },
            { Piece.P_Rook  , "RY" },
        };

        private static readonly Dictionary<Piece, string> PieceToCsa = new Dictionary<Piece, string>
        {
            { Piece.B_Pawn     , "+FU" },
            { Piece.B_Lance    , "+KY" },
            { Piece.B_Knight   , "+KE" },
            { Piece.B_Silver   , "+GI" },
            { Piece.B_Gold     , "+KI" },
            { Piece.B_Bishop   , "+KA" },
            { Piece.B_Rook     , "+HI" },
            { Piece.B_King     , "+OU" },
            { Piece.B_P_Pawn  , "+TO" },
            { Piece.B_P_Lance , "+NY" },
            { Piece.B_P_Knight, "+NK" },
            { Piece.B_P_Silver, "+NG" },
            { Piece.B_P_Bishop, "+UM" },
            { Piece.B_P_Rook  , "+RY" },
            { Piece.W_Pawn     , "-FU" },
            { Piece.W_Lance    , "-KY" },
            { Piece.W_Knight   , "-KE" },
            { Piece.W_Silver   , "-GI" },
            { Piece.W_Gold     , "-KI" },
            { Piece.W_Bishop   , "-KA" },
            { Piece.W_Rook     , "-HI" },
            { Piece.W_King     , "-OU" },
            { Piece.W_P_Pawn  , "-TO" },
            { Piece.W_P_Lance , "-NY" },
            { Piece.W_P_Knight, "-NK" },
            { Piece.W_P_Silver, "-NG" },
            { Piece.W_P_Bishop, "-UM" },
            { Piece.W_P_Rook  , "-RY" },
        };
        
        /// <summary>
        /// USI 形式の文字列に変換
        /// </summary>
        public static string Usi(this Piece p)
        {
	        var t = p.Colorless() != Piece.King
		        ? p.Demoted() : p;
	        if (!PieceToChar.ContainsKey(t))
	        {
		        throw new FormatException($"Piece: {p} が有効な値ではありません");
	        }
	        char c = PieceToChar[t];
	        return p.Colorless() != Piece.King && p.IsPromoted()
		        ? $"+{c}" : $"{c}";
        }

        /// <summary>
        /// CSA 形式の文字列に変換
        /// </summary>
        public static string Csa(this Piece p)
        {
	        if (!PieceToCsa.ContainsKey(p))
	        {
		        throw new FormatException($"Piece: {p} が有効な値ではありません");
	        }
	        return PieceToCsa[p];
        }

        /// <summary>
        /// CSA 形式の文字列（符号なし）に変換
        /// </summary>
        public static string CsaNoColor(this Piece p)
        {
	        p = p.Colorless();
	        if (!PieceToCsaNoColor.ContainsKey(p))
	        {
		        throw new FormatException($"Piece: {p} が有効な値ではありません");
	        }
	        return PieceToCsaNoColor[p];
        }
        
		public static string Pretty(this Piece p)
		{
			return PrettyPiece[(int)p];
		}
	}
}