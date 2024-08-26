/*
 * Copyright (c) 2022 tomori-k
 */

using System.Text;

namespace UniShogi
{
	public class Position
	{
		private string _initPos;
		private Board _board = new Board();
		
		/// <summary>
		/// 手番
		/// </summary>
		public Color Player
		{
			get => _board.Player;
			private set => _board.Player = value;
		}
		
		/// <summary>
		/// 手数
		/// </summary>
		public int Turn { get; private set; } = 1;

		public Position()
		{
			
		}

		public Position(Board board)
		{
			_board = board.Clone();
		}
		
		public Piece PieceAt(int sq)
		{
			return _board.Squares[sq];
		}
		
		public Piece PieceAt(int rank, int file)
		{
			return _board.Squares[Square.Index(rank, file)];
		}
		
		/// <summary>
		/// c の駒台
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public ref CaptureList CaptureListOf(Color c)
		{
			return ref _board.CaptureListOf(c);
		}

		public string Sfen()
		{
			var sb = new StringBuilder();
			for (int rank = 0; rank < 9; ++rank)
			{
				if (rank != 0) sb.Append('/');
				for (int file = 8; file >= 0; --file)
				{
					int numEmpties = 0;
					for (; file >= 0; --file)
					{
						if (PieceAt(rank, file) != Piece.Empty) break;
						numEmpties += 1;
					}

					if (numEmpties > 0)
					{
						sb.Append(numEmpties);
					}

					if (file >= 0)
					{
						sb.Append(PieceAt(rank, file).Usi());
					}
				}
			}
			
			sb.Append($" {Player.Usi()}");

			// 持ち駒
			if (CaptureListOf(Color.Black).Any() || CaptureListOf(Color.White).Any())
			{
				sb.Append(' ');
				foreach (Color c in Enum.GetValues(typeof(Color)))
				{
					foreach (Piece p in PieceExtensions.PawnToRook.Reverse())
					{
						int n = CaptureListOf(c).Count(p);
						var ps = p.Colored(c).Usi();
						if (n == 1)
							sb.Append(ps);
						else if (n > 1)
							sb.Append($"{n}{ps}");

					}
				}
			}
			else
			{
				sb.Append(" -");
			}
			
			// 手数
			sb.Append($" {Turn}");

			return sb.ToString();
			
		}

		public string Pretty()
		{
			var sb = new StringBuilder();
			sb.AppendLine(_board.Pretty());

			return sb.ToString();
		}
		
		
	}
}