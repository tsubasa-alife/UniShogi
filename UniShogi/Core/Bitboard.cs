/*
 * Copyright (c) 2022 tomori-k
 * This file is modified from the original file by Tsubasa Hizono
 */

using System.Numerics;
using System.Text;

namespace UniShogi
{
	/// <summary>
    /// 駒があるかないかのみを表すデータ構造                 <br/>
    /// ビットと盤面の対応                                 <br/>
    /// 9  8        7  6  5  4  3  2  1                   <br/>
    /// 09 00       54 45 36 27 18 09 00 一               <br/>
    /// 10 01       55 46 37 28 19 10 01 二               <br/>
    /// 11 02       56 47 38 29 20 11 02 三      ↑ RIGHT  <br/>
    /// 12 03       57 48 39 30 21 12 03 四 UP ←   → DOWN <br/>
    /// 13 04       58 49 40 31 22 13 04 五      ↓ LEFT   <br/>
    /// 14 05       59 50 41 32 23 14 05 六               <br/>
    /// 15 06       60 51 42 33 24 15 06 七               <br/>
    /// 16 07       61 52 43 34 25 16 07 八               <br/>
    /// 17 08       62 53 44 35 26 17 08 九               <br/>
    ///    hi                         lo                  <br/>
    /// </summary>
    public readonly struct Bitboard : IEnumerable<int>
    {
        #region テーブル

        static readonly Bitboard[] REACHABLE_MASK = new Bitboard[8 * 2];
        static readonly Bitboard[] SQUARE_BIT = new Bitboard[81];
        static readonly Bitboard[] PAWN_ATTACKS = new Bitboard[81 * 2];
        static readonly Bitboard[] KNIGHT_ATTACKS = new Bitboard[81 * 2];
        static readonly Bitboard[] SILVER_ATTACKS = new Bitboard[81 * 2];
        static readonly Bitboard[] GOLD_ATTACKS = new Bitboard[81 * 2];
        static readonly Bitboard[] KING_ATTACKS = new Bitboard[81];
        static readonly Bitboard[] LANCE_PSEUDO_ATTACKS = new Bitboard[81 * 2];
        static readonly Bitboard[] BISHOP_PSEUDO_ATTACKS = new Bitboard[81];
        static readonly Bitboard[] ROOK_PSEUDO_ATTACKS = new Bitboard[81];

        static readonly Bitboard[] RAY_BB = new Bitboard[81 * 8]; // LEFT, LEFTUP, UP, RIGHTUP, RIGHT, RIGHTDOWN, DOWN, LEFTDOWN

        #endregion

        readonly UInt128 _x;

        public Bitboard(UInt64 lo, UInt64 hi)
        {
            this._x = new UInt128(hi, lo);
        }

        public Bitboard(UInt128 x)
        {
            this._x = x;
        }

        public Bitboard(string bitPattern)
        {
            this._x = new UInt128(0UL, 0UL);
            foreach (var (c, i)
                in bitPattern.Select((x, i) => (x, i)))
            {
                if (c != 'o') continue;
                var rank = i / 9;
                var file = 8 - i % 9;
                this |= Square.Index(rank, file);
            }
        }

        
        public static Bitboard operator&(Bitboard lhs, Bitboard rhs)
        {
            return new(lhs.Lower() & rhs.Lower(), lhs.Upper() & rhs.Upper());
        }

        
        public static Bitboard operator|(Bitboard lhs, Bitboard rhs)
        {
            return new(lhs.Lower() | rhs.Lower(), lhs.Upper() | rhs.Upper());
        }

        
        public static Bitboard operator^(Bitboard lhs, Bitboard rhs)
        {
            return new(lhs.Lower() ^ rhs.Lower(), lhs.Upper() ^ rhs.Upper());
        }

        
        public static Bitboard operator <<(Bitboard x, int shift)
        {
            return new(x.Lower() << shift, x.Upper() << shift);
        }

        
        public static Bitboard operator >>(Bitboard x, int shift)
        {
            return new(x.Lower() >> shift, x.Upper() >> shift);
        }

        
        public static Bitboard operator &(Bitboard lhs, int sq)
        {
            return lhs & SQUARE_BIT[sq];
        }

        
        public static Bitboard operator |(Bitboard lhs, int sq)
        {
            return lhs | SQUARE_BIT[sq];
        }

        
        public static Bitboard operator ^(Bitboard lhs, int sq)
        {
            return lhs ^ SQUARE_BIT[sq];
        }

        
        public static Bitboard operator~(Bitboard x)
        {
            return x ^ new Bitboard(0x7fffffffffffffffUL, 0x000000000003ffffUL);
        }

        /// <summary>
        /// this &amp; ~rhs
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        
        public Bitboard AndNot(Bitboard rhs)
        {
            return this & ~rhs;
        }

        /// <summary>
        /// 1 筋 から 7 筋までのビットボード
        /// </summary>
        /// <returns></returns>
        
        public UInt64 Lower()
        {
            return (UInt64)_x;
        }

        /// <summary>
        /// 8, 9 筋のビットボード
        /// </summary>
        /// <returns></returns>
        
        public UInt64 Upper()
        {
            return (UInt64)(_x >> 64);
        }

        /// <summary>
        /// 立っているビットの数が 0 か
        /// </summary>
        /// <returns></returns>
        
        public bool None()
        {
            return (Lower() | Upper()) == 0UL;
        }

        /// <summary>
        /// 立っているビットが存在するか
        /// </summary>
        /// <returns></returns>
        
        public bool Any()
        {
            return !None();
        }

        /// <summary>
        /// 立っているビットの数
        /// </summary>
        /// <returns></returns>
        
        public int Popcount()
        {
            return BitOperations.PopCount(Lower()) + BitOperations.PopCount(Upper());
        }

        /// <summary>
        /// LSB のビットが示すマスの番号
        /// this.None() のとき、結果は不定
        /// </summary>
        /// <returns></returns>
        
        public int LsbSquare()
        {
            return Lower() != 0UL
                ? BitOperations.TrailingZeroCount(Lower())
                : BitOperations.TrailingZeroCount(Upper()) + 63;
        }

        /// <summary>
        /// (this &amp; x).None() か
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        
        public bool TestZ(Bitboard x)
        {
            return (this & x).None();
        }

        /// <summary>
        /// sq のマスのビットが立っているか
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        
        public bool Test(int sq)
        {
            return !this.TestZ(SQUARE_BIT[sq]);
        }

        public struct Enumerator : IEnumerator<int>
        {
            bool first = true;
            ulong b0, b1;

            internal Enumerator(Bitboard x)
            {
                this.b0 = x.Lower();
                this.b1 = x.Upper();
            }

            public int Current
                => b0 != 0UL
                    ? BitOperations.TrailingZeroCount(b0)
                    : BitOperations.TrailingZeroCount(b1) + 63;

            object System.Collections.IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                if (first)
                {
                    first = false;
                    return b0 != 0UL || b1 != 0UL;
                }
                else
                {
                    if (b0 != 0UL)
                    {
                        b0 &= b0 - 1UL;
                        return b0 != 0UL || b1 != 0UL;
                    }
                    else if (b1 != 0UL)
                    {
                        b1 &= b1 - 1UL;
                        return b1 != 0UL;
                    }
                    else
                        return false;
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<int> IEnumerable<int>.GetEnumerator() => GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// sq から d の方向へ伸ばしたビットボード（sq は含まない）
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        
        public static Bitboard Ray(int sq, Direction d)
        {
            return RAY_BB[sq * 8 + (int)d];
        }

        /// <summary>
        /// ２マスの間（両端は含まない）ビットボード
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        
        public static Bitboard Between(int i, int j)
        {
            Direction d = DirectionExtensions.FromTo(i, j);
            return d != Direction.None
                ? Ray(i, d) & Ray(j, d.Reverse()) : default;
        }

        /// <summary>
        /// ２マスを通る直線のビットボード
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        
        public static Bitboard Line(int i, int j)
        {
            Direction d = DirectionExtensions.FromTo(i, j);
            return d != Direction.None
                ? Ray(i, d) | Ray(j, d.Reverse()) : default;
        }

        /// <summary>
        /// c 視点で、段 f から 段 t までを表すビットボード
        /// </summary>
        /// <param name="c"></param>
        /// <param name="f"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Bitboard Rank(Color c, int f, int t)
        {
            int from = c == Color.Black ? f : 8 - t;
            int to   = c == Color.Black ? t : 8 - f;
            ulong mul  = (1UL << (to - from + 1)) - 1UL;
            ulong low  = 0x0040201008040201UL * mul << from;
            ulong high = 0x0000000000000201UL * mul << from;
            return new(low, high);
        }

        /// <summary>
        /// c の種類 p の駒を動かせる範囲を表すビットボード
        /// </summary>
        /// <param name="c"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        
        public static Bitboard ReachableMask(Color c, Piece p)
        {
            return REACHABLE_MASK[(int)p * 2 + (int)c];
        }

        /// <summary>
        /// 歩を打てる場所を表すビットボードを計算
        /// </summary>
        /// <param name="pawns"></param>
        /// <returns></returns>
        public static Bitboard PawnDropMask(Bitboard pawns)
        {
            const ulong left0 = 0x4020100804020100UL;
            const ulong left1 = 0x0000000000020100UL;
            var t0 = left0 - pawns.Lower();
            var t1 = left1 - pawns.Upper();
            t0 = left0 - ((t0 & left0) >> 8);
            t1 = left1 - ((t1 & left1) >> 8);
            return new(left0 ^ t0, left1 ^ t1);
        }

        
        static int TableIndex(Color c, int sq) => sq * 2 + (int)c;

        
        public static Bitboard PawnAttacks(Color c, int sq)
        {
            return PAWN_ATTACKS[TableIndex(c, sq)];
        }

        
        public static Bitboard KnightAttacks(Color c, int sq)
        {
            return KNIGHT_ATTACKS[TableIndex(c, sq)];
        }

        
        public static Bitboard SilverAttacks(Color c, int sq)
        {
            return SILVER_ATTACKS[TableIndex(c, sq)];
        }

        
        public static Bitboard GoldAttacks(Color c, int sq)
        {
            return GOLD_ATTACKS[TableIndex(c, sq)];
        }

        
        public static Bitboard KingAttacks(int sq)
        {
            return KING_ATTACKS[sq];
        }

        
        public static Bitboard LancePseudoAttacks(Color c, int sq)
        {
            return LANCE_PSEUDO_ATTACKS[TableIndex(c, sq)];
        }

        
        public static Bitboard BishopPseudoAttacks(int sq)
        {
            return BISHOP_PSEUDO_ATTACKS[sq];
        }

        
        public static Bitboard RookPseudoAttacks(int sq)
        {
            return ROOK_PSEUDO_ATTACKS[sq];
        }

        /// <summary>
        /// 先手の香車の利きを計算
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="occupancy"></param>
        /// <returns></returns>
        public static Bitboard LanceAttacksBlack(int sq, Bitboard occupancy)
        {
            var mask = sq < 63
                ? Ray(sq, Direction.Right).Lower()
                : Ray(sq, Direction.Right).Upper();
            var occ = sq < 63
                ? occupancy.Lower() : occupancy.Upper();
            var masked = occ & mask;
            masked |= masked >> 1;
            masked |= masked >> 2;
            masked |= masked >> 4;
            masked >>= 1;
            return sq < 63
                ? new(~masked & mask, 0UL)
                : new(0UL, ~masked & mask);
        }

        /// <summary>
        /// 後手の香車の利きを計算
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="occupancy"></param>
        /// <returns></returns>
        public static Bitboard LanceAttacksWhite(int sq, Bitboard occupancy)
        {
            var mask = sq < 63 ? Ray(sq, Direction.Left).Lower() : Ray(sq, Direction.Left).Upper();
            var occ = sq < 63 ? occupancy.Lower() : occupancy.Upper();
            var masked = occ & mask;
            var a = (masked ^ (masked - 1)) & mask;
            return sq < 63 ? new(a, 0UL) : new(0UL, a);
        }

        /// <summary>
        /// 香車の利きを計算
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sq"></param>
        /// <param name="occupancy"></param>
        /// <returns></returns>
        public static Bitboard LanceAttacks(Color c, int sq, Bitboard occupancy)
        {
            return c == Color.Black
                ? LanceAttacksBlack(sq, occupancy)
                : LanceAttacksWhite(sq, occupancy);
        }

        /// <summary>
        /// 角の利きを計算
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="occupancy"></param>
        /// <returns></returns>
        public static Bitboard BishopAttacks(int sq, Bitboard occupancy)
        {
            return SliderAttacks_NoSse_LsbToMsb(sq, Direction.LeftUp, occupancy)
                   | SliderAttacks_NoSse_LsbToMsb(sq, Direction.RightUp, occupancy)
                   | SliderAttacks_NoSse_MsbToLsb(sq, Direction.LeftDown, occupancy)
                   | SliderAttacks_NoSse_MsbToLsb(sq, Direction.RightDown, occupancy);
        }

        /// <summary>
        /// 飛車の利きを計算
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="occupancy"></param>
        /// <returns></returns>
        public static Bitboard RookAttacks(int sq, Bitboard occupancy)
        {
            return SliderAttacks_NoSse_LsbToMsb(sq, Direction.Up, occupancy)
                   | SliderAttacks_NoSse_MsbToLsb(sq, Direction.Down, occupancy)
                   | LanceAttacksBlack(sq, occupancy)
                   | LanceAttacksWhite(sq, occupancy);
        }

        /// <summary>
        /// 利き計算
        /// </summary>
        /// <param name="p"></param>
        /// <param name="sq"></param>
        /// <param name="occupancy"></param>
        /// <returns></returns>
        public static Bitboard Attacks(Piece p, int sq, Bitboard occupancy)
        {
            return p switch
            {
                Piece.B_Pawn => PAWN_ATTACKS[sq * 2],
                Piece.B_Lance => LanceAttacksBlack(sq, occupancy),
                Piece.B_Knight => KNIGHT_ATTACKS[sq * 2],
                Piece.B_Silver => SILVER_ATTACKS[sq * 2],
                Piece.B_Gold => GOLD_ATTACKS[sq * 2],
                Piece.B_Bishop => BishopAttacks(sq, occupancy),
                Piece.B_Rook => RookAttacks(sq, occupancy),
                Piece.B_King => KING_ATTACKS[sq],
                Piece.B_P_Pawn or Piece.B_P_Lance or Piece.B_P_Knight or Piece.B_P_Silver => GOLD_ATTACKS[sq * 2],
                Piece.B_P_Bishop => BishopAttacks(sq, occupancy) | KING_ATTACKS[sq],
                Piece.B_P_Rook => RookAttacks(sq, occupancy) | KING_ATTACKS[sq],
                Piece.W_Pawn => PAWN_ATTACKS[sq * 2 + 1],
                Piece.W_Lance => LanceAttacksWhite(sq, occupancy),
                Piece.W_Knight => KNIGHT_ATTACKS[sq * 2 + 1],
                Piece.W_Silver => SILVER_ATTACKS[sq * 2 + 1],
                Piece.W_Gold => GOLD_ATTACKS[sq * 2 + 1],
                Piece.W_Bishop => BishopAttacks(sq, occupancy),
                Piece.W_Rook => RookAttacks(sq, occupancy),
                Piece.W_King => KING_ATTACKS[sq],
                Piece.W_P_Pawn or Piece.W_P_Lance or Piece.W_P_Knight or Piece.W_P_Silver => GOLD_ATTACKS[sq * 2 + 1],
                Piece.W_P_Bishop => BishopAttacks(sq, occupancy) | KING_ATTACKS[sq],
                Piece.W_P_Rook => RookAttacks(sq, occupancy) | KING_ATTACKS[sq],
                _ => new Bitboard(),
            };
        }

        /// <summary>
        /// 利き計算
        /// </summary>
        /// <param name="c"></param>
        /// <param name="p"></param>
        /// <param name="sq"></param>
        /// <param name="occupancy"></param>
        /// <returns></returns>
        public static Bitboard Attacks(Color c, Piece p, int sq, Bitboard occupancy)
        {
            return Attacks(p.Colored(c), sq, occupancy);
        }

        private static Bitboard SquareBit(int rank, int file)
        {
            return 0 <= rank && rank < 9 && 0 <= file && file < 9
                ? SQUARE_BIT[Square.Index(rank, file)] : default;
        }

        private static ulong Bswap64(ulong x)
        {
            ulong t = (x >> 32) | (x << 32);
            t = (t >> 16 & 0x0000ffff0000ffffUL) | ((t & 0x0000ffff0000ffffUL) << 16);
            t = (t >> 8 & 0x00ff00ff00ff00ffUL) | ((t & 0x00ff00ff00ff00ffUL) << 8);
            return t;
        }

        private static Bitboard SliderAttacks_NoSse_LsbToMsb(int sq, Direction d, Bitboard occupancy)
        {
            var mask0 = Ray(sq, d).Lower();
            var mask1 = Ray(sq, d).Upper();
            var occ0 = occupancy.Lower();
            var occ1 = occupancy.Upper();
            var masked0 = occ0 & mask0;
            var masked1 = occ1 & mask1;
            var t0 = masked0 - 1UL;
            var t1 = masked1 - Convert.ToUInt64(masked0 == 0);
            t0 ^= masked0;
            t1 ^= masked1;
            t0 &= mask0;
            t1 &= mask1;
            return new Bitboard(t0, t1);
        }

        private static Bitboard SliderAttacks_NoSse_MsbToLsb(int sq, Direction d, Bitboard occupancy)
        {
            var mask0 = Ray(sq, d).Lower();
            var mask1 = Ray(sq, d).Upper();
            var occ0 = occupancy.Lower();
            var occ1 = occupancy.Upper();
            var masked0 = Bswap64(occ1 & mask1);
            var masked1 = Bswap64(occ0 & mask0);
            var t0 = masked0 - 1UL;
            var t1 = masked1 - Convert.ToUInt64(masked0 == 0);
            (t0, t1) = (Bswap64(t1 ^ masked1), Bswap64(t0 ^ masked0));
            t0 &= mask0;
            t1 &= mask1;
            return new Bitboard(t0, t1);
        }

        private static UInt128 Bswap128_NoSse(UInt128 x)
        {
            return new(Bswap64((UInt64)(x >> 64)), Bswap64((UInt64)x));
        }

        /// <summary>
        /// 人が読みやすい文字列に変換
        /// </summary>
        /// <returns></returns>
        public string Pretty()
        {
            var sb = new StringBuilder();
            sb.AppendLine("  ９ ８ ７ ６ ５ ４ ３ ２ １");
            for (int rank = 0; rank < 9; ++rank)
            {
                for (int file = 8; file >= 0; --file)
                {
                    sb.Append(
                        this.Test(Square.Index(rank, file)) ? " ◯" : "   ");
                }
                sb.AppendLine(Square.PrettyRank(rank));
            }
            return sb.ToString();
        }

        static Bitboard()
        {
            for (int rank = 0; rank < 9; ++rank)
                for (int file = 0; file < 9; ++file) {
                    SQUARE_BIT[Square.Index(rank, file)] = Square.Index(rank, file) < 63
                        ? new(1UL << Square.Index(rank, file), 0UL)
                        : new(0UL, 1UL << (Square.Index(rank, file) - 63));
                }

            for (int sq = 0; sq < 81; ++sq)
            {
                var dr = new[] { 1, 1, 0, -1, -1, -1, 0, 1 };
                var df = new[] { 0, 1, 1, 1, 0, -1, -1, -1 };
                for (int d = 0; d < 8; ++d)
                {
                    var rank = Square.RankOf(sq);
                    var file = Square.FileOf(sq);
                    while (true)
                    {
                        rank += dr[d]; file += df[d];

                        if (!(0 <= rank && rank < 9 && 0 <= file && file < 9))
                            break;

                        RAY_BB[sq * 8 + d] |= Square.Index(rank, file);
                    }
                }
            }

            for (int rank = 0; rank < 9; ++rank)
                for (int file = 0; file < 9; ++file)
                {
                    PAWN_ATTACKS[Square.Index(rank, file) * 2 + (int)Color.Black]
                        = SquareBit(rank - 1, file);
                    PAWN_ATTACKS[Square.Index(rank, file) * 2 + (int)Color.White]
                        = SquareBit(rank + 1, file);
                    KNIGHT_ATTACKS[Square.Index(rank, file) * 2 + (int)Color.Black]
                        = SquareBit(rank - 2, file - 1) | SquareBit(rank - 2, file + 1);
                    KNIGHT_ATTACKS[Square.Index(rank, file) * 2 + (int)Color.White]
                        = SquareBit(rank + 2, file - 1) | SquareBit(rank + 2, file + 1);
                    SILVER_ATTACKS[Square.Index(rank, file) * 2 + (int)Color.Black]
                        = SquareBit(rank - 1, file - 1)
                        | SquareBit(rank - 1, file)
                        | SquareBit(rank - 1, file + 1)
                        | SquareBit(rank + 1, file - 1)
                        | SquareBit(rank + 1, file + 1);
                    SILVER_ATTACKS[Square.Index(rank, file) * 2 + (int)Color.White]
                        = SquareBit(rank + 1, file - 1)
                        | SquareBit(rank + 1, file)
                        | SquareBit(rank + 1, file + 1)
                        | SquareBit(rank - 1, file - 1)
                        | SquareBit(rank - 1, file + 1);
                    GOLD_ATTACKS[Square.Index(rank, file) * 2 + (int)Color.Black]
                        = SquareBit(rank - 1, file - 1)
                        | SquareBit(rank - 1, file)
                        | SquareBit(rank - 1, file + 1)
                        | SquareBit(rank, file - 1)
                        | SquareBit(rank, file + 1)
                        | SquareBit(rank + 1, file);
                    GOLD_ATTACKS[Square.Index(rank, file) * 2 + (int)Color.White]
                        = SquareBit(rank + 1, file - 1)
                        | SquareBit(rank + 1, file)
                        | SquareBit(rank + 1, file + 1)
                        | SquareBit(rank, file - 1)
                        | SquareBit(rank, file + 1)
                        | SquareBit(rank - 1, file);
                }

            for (int i = 0; i < 81; ++i)
                KING_ATTACKS[i] = SILVER_ATTACKS[i * 2] | GOLD_ATTACKS[i * 2];

            foreach (var p in PieceExtensions.PawnToRook)
            {
                foreach (Color c in new[] { Color.Black, Color.White})
                {
                    REACHABLE_MASK[(int)p * 2 + (int)c] =
                        p == Piece.Pawn || p == Piece.Lance ? Rank(c, 1, 8)
                      : p == Piece.Knight                   ? Rank(c, 2, 8)
                      :                                       Rank(c, 0, 8);
                }
            }

            for (int i = 0; i < 81; ++i)
            {
                LANCE_PSEUDO_ATTACKS[i * 2 + 0] = LanceAttacksBlack(i, default);
                LANCE_PSEUDO_ATTACKS[i * 2 + 1] = LanceAttacksWhite(i, default);
                BISHOP_PSEUDO_ATTACKS[i] = BishopAttacks(i, default);
                ROOK_PSEUDO_ATTACKS[i] = RookAttacks(i, default);
            }
        }
    }
}