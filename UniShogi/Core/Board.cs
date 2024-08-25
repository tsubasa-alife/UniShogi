using System.Text;

namespace UniShogi
{
	public class Board
	{
		public Piece[] Squares { get; } = new Piece[81];
		
		public Board()
		{
			
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