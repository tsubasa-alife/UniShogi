/*
 * Copyright (c) 2022 tomori-k
 */

namespace UniShogi
{
	public static class Csa
    {
        /// <summary>
        /// CSA 形式の棋譜の開始局面をパース
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="FormatException"></exception>
        public static Board ParseBoard(string csaBoard)
        {
            var lines = new Queue<string>(csaBoard
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !x.StartsWith("'"))
                .Select(x => x.Split(','))
                .SelectMany(x => x));
            return ParseBoard(lines);
        }

        /// <summary>
        /// CSA 形式の棋譜の開始局面をパース
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="FormatException"></exception>
        public static Board ParseBoard(Queue<string> lines)
        {
            var board = new Board();

            if (lines.TryPeek(out var firstLine) && firstLine.StartsWith("PI"))
            {
                // todo
                throw new NotImplementedException();
            }
            else
            {
                // 一括表現
                for (int rank = 0; rank < 9; ++rank)
                {
                    if (!lines.TryDequeue(out var line) || line.Length < 3 * 9 + 2)
                    {
                        throw new FormatException($"盤面情報が欠けています：{line}");
                    }

                    for (int file = 0; file < 9; ++file)
                    {
                        var sq = Square.Index(rank, file);
                        var pieceStr = line.Substring(2 + (8 - file) * 3, 3);

                        if (pieceStr != " * ")
                        {
                            board.Squares[sq] = ParsePiece(pieceStr);
                        }
                    }
                }
                // 駒別単独表現
                while (true)
                {
                    if (!lines.TryPeek(out var next) || !next.StartsWith("P")) break;

                    lines.Dequeue();

                    var pc = next.StartsWith("P+")
                        ? Color.Black : Color.White;

                    for (int i = 2; i + 3 < next.Length; i += 4)
                    {
                        var squareStr = next.Substring(i, 2);
                        var pieceStr = next.Substring(i + 2, 2);
                        if (!TryParsePiece(pieceStr, out var piece))
                        {
                            throw new FormatException($"駒の形式が正しくありません：{next}");
                        }
                        // todo: AL対応
                        if (squareStr == "00")
                        {
                            board.CaptureListOf(pc).Add(piece, 1);
                        }
                        else
                        {
                            if (!TryParseSquare(squareStr, out var sq))
                            {
                                throw new FormatException($"駒の位置が正しくありません：{next}");
                            }
                            board.Squares[sq] = piece.Colored(pc);
                        }
                    }
                }
            }

            // 手番
            if (!lines.TryDequeue(out var colorStr))
            {
                throw new FormatException("開始局面での手番の情報がありません。");
            }
            board.Player = colorStr == "+" ? Color.Black : Color.White;

            return board;
        }

        public static (Move, TimeSpan) ParseMoveWithTime(string s, Position pos)
        {
            var sp = s.Split(',')
                .Select(x => x.Trim())
                .ToArray();
            if (sp.Length < 2
                || !TryParseMove(sp[0], pos, out var move)
                || !TryParseTime(sp[1], out var time))
            {
                throw new FormatException($"指し手の形式が正しくありません。：{s}");
            }
            return (move, time);
        }

        public static TimeSpan ParseTime(string timeStr)
        {
            if (!TryParseTime(timeStr, out var time))
            {
                throw new FormatException("消費時間の形式が正しくありません。：");
            }
            return time;
        }

        public static bool TryParseTime(string timeStr, out TimeSpan time)
        {
            if (timeStr.StartsWith("T")
                && int.TryParse(timeStr[1..], out var sec))
            {
                time = TimeSpan.FromSeconds(sec);
                return true;
            }
            else
            {
                time = TimeSpan.Zero;
                return false;
            }
        }

        /// <summary>
        /// CSA 形式の棋譜で用いられる指し手文字列をパース
        /// </summary>
        /// <param name="moveStr"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static Move ParseMove(string moveStr, Position pos)
        {
            if (!TryParseMove(moveStr, pos, out var move))
            {
                throw new FormatException($"CSA 形式の指し手文字列ではありません：{moveStr}");
            }
            return move;
        }

        /// <summary>
        /// CSA 形式の棋譜で用いられる指し手文字列をパース
        /// </summary>
        /// <param name="moveStr"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static bool TryParseMove(string moveStr, Position pos, out Move move)
        {
            if (moveStr.Length < 7
                || !TryParseSquare(moveStr[3..5], out var to))
            {
                move = Move.None;
                return false;
            }
            // 駒打ち
            if (moveStr[1..3] == "00")
            {
                if (TryParsePiece(moveStr[5..7], out var piece))
                {
                    move = MoveExtensions.MakeDrop(piece, to);
                    return true;
                }
                else
                {
                    move = Move.None;
                    return false;
                }
            }
            else
            {
                if (!TryParseSquare(moveStr[1..3], out var from))
                {
                    move = Move.None;
                    return false;
                }

                var pieceAfterStr = moveStr[0] + moveStr[5..7];
                if (!TryParsePiece(pieceAfterStr, out var pieceAfter))
                {
                    move = Move.None;
                    return false;
                }

                var promote = !pos.PieceAt(from).IsPromoted()
                    && pieceAfter.IsPromoted();
                move = MoveExtensions.MakeMove(from, to, promote);

                return true;
            }
        }

        /// <summary>
        /// CSA 形式の棋譜で用いられる座標文字列をパース
        /// </summary>
        /// <param name="squareStr"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static int ParseSquare(string squareStr)
        {
            if (TryParseSquare(squareStr, out var sq))
            {
                throw new FormatException($"CSA 形式の座標文字列ではありません：{squareStr}");
            }
            return sq;
        }

        /// <summary>
        /// CSA 形式の棋譜で用いられる座標文字列をパース
        /// </summary>
        /// <param name="squareStr"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static bool TryParseSquare(string squareStr, out int v)
        {
            if (squareStr.Length >= 2
                && ('1' <= squareStr[0] && squareStr[0] <= '9')
                && ('1' <= squareStr[1] && squareStr[1] <= '9'))
            {
                v = Square.Index(squareStr[1] - '1', squareStr[0] - '1');
                return true;
            }
            else
            {
                v = 0;
                return false;
            }
        }

        private static readonly Dictionary<string, Piece> CsaToPiece = new()
        {
            { "FU", Piece.Pawn},
            { "KY", Piece.Lance },
            { "KE", Piece.Knight },
            { "GI", Piece.Silver },
            { "KI", Piece.Gold },
            { "KA", Piece.Bishop },
            { "HI", Piece.Rook },
            { "OU", Piece.King },
            { "TO", Piece.P_Pawn},
            { "NY", Piece.P_Lance },
            { "NK", Piece.P_Knight },
            { "NG", Piece.P_Silver },
            { "UM", Piece.P_Bishop },
            { "RY", Piece.P_Rook },
            { "+FU", Piece.B_Pawn},
            { "+KY", Piece.B_Lance },
            { "+KE", Piece.B_Knight },
            { "+GI", Piece.B_Silver },
            { "+KI", Piece.B_Gold },
            { "+KA", Piece.B_Bishop },
            { "+HI", Piece.B_Rook },
            { "+OU", Piece.B_King },
            { "+TO", Piece.B_P_Pawn},
            { "+NY", Piece.B_P_Lance },
            { "+NK", Piece.B_P_Knight },
            { "+NG", Piece.B_P_Silver },
            { "+UM", Piece.B_P_Bishop },
            { "+RY", Piece.B_P_Rook },
            { "-FU", Piece.W_Pawn},
            { "-KY", Piece.W_Lance },
            { "-KE", Piece.W_Knight },
            { "-GI", Piece.W_Silver },
            { "-KI", Piece.W_Gold },
            { "-KA", Piece.W_Bishop },
            { "-HI", Piece.W_Rook },
            { "-OU", Piece.W_King },
            { "-TO", Piece.W_P_Pawn},
            { "-NY", Piece.W_P_Lance },
            { "-NK", Piece.W_P_Knight },
            { "-NG", Piece.W_P_Silver },
            { "-UM", Piece.W_P_Bishop },
            { "-RY", Piece.W_P_Rook },
        };

        /// <summary>
        /// CSA 形式の棋譜で用いられる駒文字列をパース
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static Piece ParsePiece(string ps)
        {
            if (!TryParsePiece(ps, out var piece))
            {
                throw new FormatException($"CSA 形式の駒文字列ではありません：{ps}");
            }
            return piece;
        }

        /// <summary>
        /// CSA 形式の棋譜で用いられる駒文字列をパース
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static bool TryParsePiece(string ps, out Piece v)
        {
            if (CsaToPiece.ContainsKey(ps))
            {
                v = CsaToPiece[ps];
                return true;
            }
            else
            {
                v = Piece.Empty;
                return false;
            }
        }

        public static string ToSquare(int sq)
        {
            if (!(0 <= sq && sq < 81))
            {
                throw new FormatException($"駒の位置が盤面に収まっていません。: {sq}");
            }
            return $"{Square.FileOf(sq) + 1}{Square.RankOf(sq) + 1}";
        }

        public static bool TryParseColor(string s, out Color c)
        {
            if (s == "+" || s == "-")
            {
                c = s == "+" ? Color.Black : Color.White;
                return true;
            }
            else
            {
                c = Color.Black;
                return false;
            }
        }

        public static Color ParseColor(string s)
        {
            if (!TryParseColor(s, out var c))
            {
                throw new FormatException($"CSA 形式ではありません。: {s}");
            }
            return c;
        }
        
        // CSA 棋譜ファイル形式：http://www2.computer-shogi.org/protocol/record_v22.html

        /// <summary>
        /// CSA 形式の棋譜をパース
        /// </summary>
        /// <exception cref="FormatException"></exception>
        public static Kifu Parse(string path)
        {
            using var reader = new StreamReader(path);
            return Parse(reader);
        }

        /// <summary>
        /// CSA 形式の棋譜をパース
        /// </summary>
        /// <exception cref="FormatException"></exception>
        public static Kifu Parse(TextReader textReader)
        {
            using var reader = new PeekableReader(textReader, '\'');
            ParseVersion(reader);
            var (nameBlack, nameWhite) = ParseNames(reader);
            var info = ParseGameInfo(reader);
            var startpos = ParseStartPos(reader);
            var moves = ParseMoves(reader, startpos);
            // ParseResult(lines);
            return new Kifu(info, startpos, new() { new MoveSequence(1, moves) });
        }

        static void ParseVersion(PeekableReader reader)
        {
            var version = reader.ReadLine();
            if (version != "V2.2")
            {
                throw new FormatException("V2.2以外のバージョンのフォーマットはサポートしていません");
            }
        }

        const string PrefixBlack = "N+";
        const string PrefixWhite = "N-";

        /// <summary>
        /// CSA 形式の棋譜の対局者情報をパース
        /// </summary>
        static (string?, string?) ParseNames(PeekableReader reader)
        {
            string? nameBlack = null;
            string? nameWhite = null;
            if (reader.PeekLine() is { } b && b.StartsWith(PrefixBlack))
            {
                nameBlack = b[PrefixBlack.Length..];
                reader.ReadLine();
            }
            if (reader.PeekLine() is { } w && w.StartsWith(PrefixWhite))
            {
                nameWhite = w[PrefixWhite.Length..];
                reader.ReadLine();
            }
            return (nameBlack, nameWhite);
        }

        const string PrefixEvent = "$EVENT:";
        const string PrefixSite = "$SITE:";
        const string PrefixStartTime = "$START_TIME:";
        const string PrefixEndTime = "$END_TIME:";
        const string PrefixTimeLimit = "$TIME_LIMIT:";
        const string PrefixOpening = "$OPENING:";

        /// <summary>
        /// CSA 形式の棋譜の棋譜情報をパース
        /// </summary>
        static GameInfo ParseGameInfo(PeekableReader reader)
        {
            var info = new GameInfo();
            while (true)
            {
                var line = reader.PeekLine();
                if (line is null || !line.StartsWith("$")) break;

                reader.ReadLine();

                if (line.StartsWith(PrefixEvent))
                {
                    info.Event = line[PrefixEvent.Length..];
                }
                else if (line.StartsWith(PrefixSite))
                {
                    info.Site = line[PrefixSite.Length..];
                }
                else if (line.StartsWith(PrefixStartTime)
                    && DateTime.TryParse(line[PrefixStartTime.Length..], out var startTime))
                {
                    info.StartTime = startTime;
                }
                else if (line.StartsWith(PrefixEndTime)
                    && DateTime.TryParse(line[PrefixEndTime.Length..], out var endTime))
                {
                    info.EndTime = endTime;
                }
                else if (line.StartsWith(PrefixTimeLimit))
                {
                    // todo
                }
                else if (line.StartsWith(PrefixOpening))
                {
                    info.Opening = line[PrefixOpening.Length..];
                }
            }
            return info;
        }

        static Board ParseStartPos(PeekableReader reader)
        {
            var lines = new Queue<string>();
            while (true)
            {
                if (reader.PeekLine() is not { } line
                    || !line.StartsWith('P')) break;
                reader.ReadLine();
                lines.Enqueue(line);
            }
            if (lines.Count == 0)
            {
                throw new FormatException("開始局面の情報がありません。");
            }
            var colorStr = reader.ReadLine();
            if (colorStr is null
                || !(colorStr == "+" || colorStr == "-"))
            {
                throw new FormatException("開始局面の手番情報がありません。");
            }
            lines.Enqueue(colorStr);
            return ParseBoard(lines);
        }

        /// <summary>
        /// CSA 形式の棋譜の指し手・消費時間をパース
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="startpos"></param>
        /// <returns></returns>
        static List<MoveInfo> ParseMoves(PeekableReader reader, Board startpos)
        {
            var pos = new Position(startpos);
            var moves = new List<MoveInfo>();

            while (true)
            {
                var moveStr = reader.ReadLine();
                if (moveStr is null
                    || !(moveStr.StartsWith("+") || moveStr.StartsWith("-"))) break;

                var move = ParseMove(moveStr, pos);
                pos.DoMove(move);

                // 消費時間はオプションなので、ないこともある

                if (reader.PeekLine() is { } timeStr && timeStr.StartsWith("T"))
                {
                    reader.ReadLine();
                    moves.Add(new(move, ParseTime(timeStr)));
                }
                else 
                    moves.Add(new(move, null));
            }

            return moves;
        }

        /// <summary>
        /// CSA 形式の棋譜の終局状況をパース
        /// </summary>
        /// <param name="lines"></param>
        /// <exception cref="NotImplementedException"></exception>
        public static void ParseResult(Queue<string> lines)
        {
            throw new NotImplementedException();
        }
    }
}