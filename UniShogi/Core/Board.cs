/*
 * Copyright (c) 2022 tomori-k
 */

using System.Text;

namespace UniShogi
{
	public class Board
	{
		public Color Player { get; set; }
		public Piece[] Squares { get; } = new Piece[81];
		public CaptureList[] CaptureLists { get; } = new CaptureList[2];
		
		public Board()
		{
			
		}

		public Board(Board board)
		{
			Squares = (Piece[])board.Squares.Clone();
		}
		
		/// <summary>
		/// c の駒台
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public ref CaptureList CaptureListOf(Color c)
		{
			return ref CaptureLists[(int)c];
		}

		public Board Clone()
		{
			return new Board(this);
		}
		
		public string Pretty()
		{
			var sb = new StringBuilder();
			
			sb.AppendLine("  ９ ８ ７ ６ ５ ４ ３ ２ １");
			sb.AppendLine("+-------------------------+");

			for (int rank = 0; rank < 9; ++rank)
			{
				sb.Append("|");

				for (int file = 8; file >= 0; --file)
				{
					sb.Append($"{Squares[Square.Index(rank, file)].Pretty(),2}");
				}

				sb.AppendLine($"|{Square.PrettyRank(rank)}");
			}

			sb.AppendLine("+-------------------------+");

			return sb.ToString();
		}
		
	}
}