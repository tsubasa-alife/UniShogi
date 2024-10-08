/*
 * Copyright (c) 2022 tomori-k
 */

namespace UniShogi
{
	/// <summary>
    /// 指し手
    /// </summary>
    public enum Move : ushort
    {
        None = 0,
        Win = 2 + (2 << 7),
        Resign = 3 + (3 << 7),
        ToMask = 0x7f,
        PromoteBit = 0x4000,
        DropBit = 0x8000,
    }

    public static class MoveExtensions
    {
        /// <summary>
        /// 移動先
        /// </summary>
        /// <returns></returns>
        public static int To(this Move m)
        {
            return (int)(m & Move.ToMask);
        }

        /// <summary>
        /// 移動元
        /// </summary>
        /// <returns></returns>
        public static int From(this Move m)
        {
            return (int)m >> 7 & (int)Move.ToMask;
        }

        /// <summary>
        /// 打つ駒の種類
        /// </summary>
        /// <returns>Pawn, Lance, Knight, Silver, Gold, Bishop, Rook のどれか</returns>
        public static Piece Dropped(this Move m)
        {
            return (Piece)m.From();
        }

        /// <summary>
        /// 成る指し手か
        /// </summary>
        /// <returns></returns>
        public static bool IsPromote(this Move m)
        {
            return (m & Move.PromoteBit) != 0;
        }

        /// <summary>
        /// 駒打ちか
        /// </summary>
        /// <returns></returns>
        public static bool IsDrop(this Move m)
        {
            return (m & Move.DropBit) != 0;
        }

        /// <summary>
        /// USI 形式の指し手文字列に変換
        /// </summary>
        /// <returns></returns>
        public static string ToUsi(this Move m)
        {
            var to = Usi.ToSquare(m.To());
            if (m.IsDrop())
            {
                return $"{m.Dropped().Usi()}*{to}";
            }
            else
            {
                var from = Usi.ToSquare(m.From());
                var promote = (m.IsPromote() ? "+" : "");
                return $"{from}{to}{promote}";
            }
        }

        /// <summary>
        /// CSA 形式の指し手文字列に変換
        /// </summary>
        /// <param name="m"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static string ToCsa(this Move m, Position pos)
        {
            var to = Csa.ToSquare(m.To());
            if (m.IsDrop())
            {
                var after = m.Dropped().Colored(pos.Player);
                return $"{pos.Player.Csa()}00{to}{after.CsaNoColor()}";
            }
            else
            {
                var from = Csa.ToSquare(m.From());
                var after = m.IsPromote()
                    ? pos.PieceAt(m.From()).Promoted()
                    : pos.PieceAt(m.From());
                return $"{pos.Player.Csa()}{from}{to}{after.CsaNoColor()}";
            }
        }

        /// <summary>
        /// from から to に動かす指し手を生成
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="promote">成る指し手かどうか</param>
        public static Move MakeMove(int from, int to, bool promote = false)
        {
            return (Move)(to + (from << 7) + (Convert.ToInt32(promote) << 14));
        }

        /// <summary>
        /// p の駒を to に打つ指し手を生成
        /// </summary>
        /// <param name="p"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static Move MakeDrop(Piece p, int to)
        {
            return (Move)(to + ((int)p << 7) + (1 << 15));
        }
    }
}