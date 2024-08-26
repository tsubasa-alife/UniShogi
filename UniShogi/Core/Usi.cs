/*
 * Copyright (c) 2022 tomori-k
 */

namespace UniShogi
{
	public static class Usi
    {
        private static readonly Dictionary<char, Piece> CharToPiece
            = new Dictionary<char, Piece> {
                {'P', Piece.B_Pawn  },
                {'L', Piece.B_Lance },
                {'N', Piece.B_Knight},
                {'S', Piece.B_Silver},
                {'G', Piece.B_Gold  },
                {'B', Piece.B_Bishop},
                {'R', Piece.B_Rook  },
                {'K', Piece.B_King  },
                {'p', Piece.W_Pawn  },
                {'l', Piece.W_Lance },
                {'n', Piece.W_Knight},
                {'s', Piece.W_Silver},
                {'g', Piece.W_Gold  },
                {'b', Piece.W_Bishop},
                {'r', Piece.W_Rook  },
                {'k', Piece.W_King  },
            };

        /// <summary>
        /// USI形式の駒文字 c を Piece に変換
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static Piece FromUsi(char c)
            => TryParsePiece(c, out var p) ? p : throw new FormatException($"文字を駒に変換できません：{c}");

        /// <summary>
        /// USI形式の駒文字 c を Piece に変換
        /// </summary>
        /// <param name="c"></param>
        public static bool TryParsePiece(char c, out Piece piece)
        {
            if (CharToPiece.ContainsKey(c))
            {
                piece = CharToPiece[c];
                return true;
            }
            else
            {
                piece = Piece.None;
                return false;
            }
        }
    }
}