/*
 * Copyright (c) 2022 tomori-k
 */

namespace UniShogi
{
	public static class Movegen
    {
        private static int currentIdx = 0;
        /// <summary>
        /// 合法手を生成する。
        /// </summary>
        public static MoveList GenerateMoves(Position pos)
        {
            currentIdx = 0;
            var moves = new Move[BufferSize];
            var end = GenerateMoves(moves, pos);
            return new MoveList(moves, currentIdx + 1);
        }

        /// <summary>
        /// 指し手バッファの長さ
        /// </summary>
        public const int BufferSize = 600;

        /// <summary>
        /// 合法手を生成する。
        /// </summary>
        public static Move[] GenerateMoves(Move[] moves, Position pos)
        {
            if (pos.InCheck()) return GenerateEvasionMoves(moves, pos);

            var occupancy = pos.GetOccupancy();
            var us = pos.ColorBB(pos.Player);
            var pinned = pos.PinnedBy(pos.Player.Opponent()) & us;

            // 歩
            {
                var fromBB = pos.PieceBB(pos.Player, Piece.Pawn).AndNot(pinned);
                var toBB = (pos.Player == Color.Black ? fromBB >> 1 : fromBB << 1).AndNot(us);
                var delta = pos.Player == Color.Black ? 1 : -1;
                foreach (var to in toBB)
                {
                    var from = to + delta;
                    var rank = Square.RankOf(pos.Player, to);
                    switch (rank)
                    {
                        case 0:
                            moves[currentIdx++] = MoveExtensions.MakeMove(from, to, true);
                            break;
                        case 1: case 2:
                            moves[currentIdx++] = MoveExtensions.MakeMove(from, to, false);
                            moves[currentIdx++] = MoveExtensions.MakeMove(from, to, true);
                            break;
                        default:
                            moves[currentIdx++] = MoveExtensions.MakeMove(from, to, false);
                            break;
                    }
                }
            }

            // 香
            {
                var fromBB = pos.PieceBB(pos.Player, Piece.Lance).AndNot(pinned);
                foreach (var from in fromBB)
                {
                    var toBB = Bitboard
                        .LanceAttacks(pos.Player, from, occupancy)
                        .AndNot(us);
                    foreach (var to in toBB)
                    {
                        var rank = Square.RankOf(pos.Player, to);
                        switch (rank)
                        {
                            case 0:
                            moves[currentIdx++] = MoveExtensions.MakeMove(from, to, true);
                            break;
                        case 1: case 2:
                            moves[currentIdx++] = MoveExtensions.MakeMove(from, to, false);
                            moves[currentIdx++] = MoveExtensions.MakeMove(from, to, true);
                            break;
                        default:
                            moves[currentIdx++] = MoveExtensions.MakeMove(from, to, false);
                            break;
                        }
                    }
                }
            }

            // 桂
            {
                var fromBB = pos.PieceBB(pos.Player, Piece.Knight).AndNot(pinned);
                foreach (var from in fromBB)
                {
                    var toBB = Bitboard
                        .KnightAttacks(pos.Player, from)
                        .AndNot(us);
                    foreach (var to in toBB)
                    {
                        var rank = Square.RankOf(pos.Player, to);
                        switch (rank)
                        {
                            case 0: case 1:
                                moves[currentIdx++] = MoveExtensions.MakeMove(from, to, true);
                                break;
                            case 2:
                                moves[currentIdx++] = MoveExtensions.MakeMove(from, to, false);
                                moves[currentIdx++] = MoveExtensions.MakeMove(from, to, true);
                                break;
                            default:
                                moves[currentIdx++] = MoveExtensions.MakeMove(from, to, false);
                                break;
                        }
                    }
                }
            }

            // 銀、角、飛
            {
                var fromBB = (pos.PieceBB(pos.Player, Piece.Silver)
                    | pos.PieceBB(pos.Player, Piece.Bishop)
                    | pos.PieceBB(pos.Player, Piece.Rook)).AndNot(pinned);

                foreach (var from in fromBB)
                {
                    var toBB = Bitboard
                        .Attacks(pos.PieceAt(from), from, occupancy)
                        .AndNot(us);
                    foreach (var to in toBB)
                    {
                        moves[currentIdx++] = MoveExtensions.MakeMove(from, to, false);
                        if (Square.CanPromote(pos.Player, from, to))
                        {
                            moves[currentIdx++] = MoveExtensions.MakeMove(from, to, true);
                        }
                    }
                }
            }

            // 玉
            {
                var from = pos.King(pos.Player);
                var toBB = Bitboard.KingAttacks(from).AndNot(us);
                foreach (var to in toBB)
                {
                    if (pos.EnumerateAttackers(
                        pos.Player.Opponent(), to).None())
                    {
                        moves[currentIdx++] = MoveExtensions.MakeMove(from, to);
                    }
                }
            }

            // その他
            {
                var fromBB = (pos.Golds(pos.Player) ^ pos.PieceBB(pos.Player, Piece.King))
                    .AndNot(pinned);
                foreach (var from in fromBB)
                {
                    var toBB = Bitboard
                        .Attacks(pos.PieceAt(from), from, occupancy)
                        .AndNot(us);
                    foreach (var to in toBB)
                    {
                        moves[currentIdx++] = MoveExtensions.MakeMove(from, to);
                    }
                }
            }

            // ピンされている駒
            if (pinned.Any())
            {
                var ksq = pos.King(pos.Player);
                foreach (var from in pinned)
                {
                    var toBB = Bitboard
                        .Attacks(pos.PieceAt(from), from, occupancy)
                        .AndNot(us) & Bitboard.Line(ksq, from);
                    foreach (var to in toBB)
                    {
                        moves = AddMovesToList(moves, pos.PieceAt(from), from, to);
                    }
                }
            }

            // 駒打ち
            return GenerateDrops(moves, pos, ~occupancy);
        }

        private static Move[] GenerateEvasionMoves(Move[] moves, Position pos)
        {
            var ksq = pos.King(pos.Player);
            var checkerCount = pos.Checkers().Popcount();

            var evasionTo = Bitboard.KingAttacks(ksq)
                .AndNot(pos.ColorBB(pos.Player));
            var occ = pos.GetOccupancy() ^ ksq;
            
            foreach (var to in evasionTo)
            {
                var canMove = pos.EnumerateAttackers(
                    pos.Player.Opponent(), to, occ)
                    .None();
                if (canMove)
                {
                    moves[currentIdx++] = MoveExtensions.MakeMove(ksq, to);
                }
            }

            if (checkerCount > 1) return moves;

            var csq = pos.Checkers().LsbSquare();
            var between = Bitboard.Between(ksq, csq);

            // 駒打ち
            moves = GenerateDrops(moves, pos, between);

            // 駒移動
            var excluded = pos.PieceBB(pos.Player, Piece.King) | pos.PinnedBy(pos.Player.Opponent());

            foreach (var to in between | pos.Checkers())
            {
                var fromBB = pos
                    .EnumerateAttackers(pos.Player, to)
                    .AndNot(excluded);
                foreach (var from in fromBB)
                {
                    moves = AddMovesToList(moves, pos.PieceAt(from), from, to);
                }
            }

            return moves;
        }

        private static Move[] GenerateDrops(Move[] moves, Position pos, Bitboard target)
        {
            var captureList = pos.CaptureListOf(pos.Player);
            if (captureList.None()) return moves;

            if (captureList.Count(Piece.Pawn) > 0)
            {
                var toBB = target
                    & Bitboard.ReachableMask(pos.Player, Piece.Pawn)
                    & Bitboard.PawnDropMask(pos.PieceBB(pos.Player, Piece.Pawn));
                {
                    var o = pos.Player.Opponent();
                    var uchifuzumeCand = Bitboard.PawnAttacks(o, pos.King(o));
                    if (!toBB.TestZ(uchifuzumeCand) && IsUchifuzume(uchifuzumeCand.LsbSquare(), pos))
                    {
                        toBB ^= uchifuzumeCand;
                    }
                }
                foreach (var to in toBB)
                {
                    moves[currentIdx++] = MoveExtensions.MakeDrop(Piece.Pawn, to);
                }
            }

            if (!captureList.ExceptPawn()) return moves;

            var tmpl = new Move[10];
            int n = 0, li = 0;

            if (captureList.Count(Piece.Knight) > 0)
            {
                tmpl[n++] = MoveExtensions.MakeDrop(Piece.Knight, 0);
                ++li;
            }
            if (captureList.Count(Piece.Lance) > 0)
            {
                tmpl[n++] = MoveExtensions.MakeDrop(Piece.Lance, 0);
            }
            int other = n;
            if (captureList.Count(Piece.Silver) > 0)
            {
                tmpl[n++] = MoveExtensions.MakeDrop(Piece.Silver, 0);
            }
            if (captureList.Count(Piece.Gold) > 0)
            {
                tmpl[n++] = MoveExtensions.MakeDrop(Piece.Gold, 0);
            }
            if (captureList.Count(Piece.Bishop) > 0)
            {
                tmpl[n++] = MoveExtensions.MakeDrop(Piece.Bishop, 0);
            }
            if (captureList.Count(Piece.Rook) > 0)
            {
                tmpl[n++] = MoveExtensions.MakeDrop(Piece.Rook, 0);
            }

            var to1 = target & Rank1BB[(int)pos.Player];
            var to2 = target & Rank2BB[(int)pos.Player];
            var rem = target & Rank39BB[(int)pos.Player];
            
            for (int i = 0; i < n; ++i)
            {
                var p = tmpl[i].Dropped();
                var toBB = Bitboard.ReachableMask(pos.Player, p) & rem;
                foreach (var to in toBB)
                {
                    moves[currentIdx++] = MoveExtensions.MakeDrop(p, to);
                }
            }
            
            for (int i = 0; i < li; ++i)
            {
                var p = tmpl[i].Dropped();
                var toBB = Bitboard.ReachableMask(pos.Player, p) & to1;
                foreach (var to in toBB)
                {
                    moves[currentIdx++] = MoveExtensions.MakeDrop(p, to);
                }
            }
            
            for (int i = li; i < other; ++i)
            {
                var p = tmpl[i].Dropped();
                var toBB = Bitboard.ReachableMask(pos.Player, p) & to2;
                foreach (var to in toBB)
                {
                    moves[currentIdx++] = MoveExtensions.MakeDrop(p, to);
                }
            }

            return moves;
        }
        
        static Move[] AddMovesToList(Move[] moves, Piece p, int from, int to)
        {
            var c = p.Color();
            p = p.Colorless();

            if ((Square.RankOf(c, to) <= 1 && p == Piece.Knight)
                || (Square.RankOf(c, to) == 0 && (p == Piece.Pawn || p == Piece.Lance)))
            {
                moves[currentIdx++] = MoveExtensions.MakeMove(from, to, true);
            }
            else
            {
                moves[currentIdx++] = MoveExtensions.MakeMove(from, to, false);

                if (Square.CanPromote(c, from, to)
                    && !(p.IsPromoted() || p == Piece.Gold || p == Piece.King))
                {
                    moves[currentIdx++] = MoveExtensions.MakeMove(from, to, true);
                }
            }

            return moves;
        }

        static bool IsUchifuzume(int to, Position pos)
        {
            var theirKsq = pos.King(pos.Player.Opponent());
            var defenders = pos.EnumerateAttackers(
                pos.Player.Opponent(), to) ^ theirKsq;

            if (defenders.Any())
            {
                var pinned = pos.PinnedBy(pos.Player);
                if (defenders.AndNot(pinned).Any())
                {
                    return false;
                }
                // 現在ピンされていても、歩を打つことで
                // ピンが解除される位置なら防御可能
                defenders &= Bitboard.Line(theirKsq, to);
                if (defenders.Any())
                {
                    return false;
                }
            }

            var occ = pos.GetOccupancy() ^ to;
            var evasionTo = Bitboard.KingAttacks(theirKsq)
                .AndNot(pos.ColorBB(pos.Player.Opponent()));
            
            foreach (var kTo in evasionTo)
            {
                var attackers = pos
                    .EnumerateAttackers(pos.Player, kTo, occ);
                if (attackers.None())
                {
                    return false;
                }
            }

            return true;
        }

        static readonly Bitboard[] Rank1BB = new Bitboard[2];
        static readonly Bitboard[] Rank2BB = new Bitboard[2];
        static readonly Bitboard[] Rank39BB = new Bitboard[2];

        static Movegen()
        {
            Rank1BB[0] = Bitboard.Rank(Color.Black, 0, 0);
            Rank2BB[0] = Bitboard.Rank(Color.Black, 1, 1);
            Rank39BB[0] = Bitboard.Rank(Color.Black, 2, 8);
            Rank1BB[1] = Bitboard.Rank(Color.White, 0, 0);
            Rank2BB[1] = Bitboard.Rank(Color.White, 1, 1);
            Rank39BB[1] = Bitboard.Rank(Color.White, 2, 8);
        }
    }
}