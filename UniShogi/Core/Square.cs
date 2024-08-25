namespace UniShogi
{
	public static class Square
	{
		private static readonly string[] PrettyRankTable = { "一", "二", "三", "四", "五", "六", "七", "八", "九" };
		private static readonly string[] PrettyFileTable = { "１", "２", "３", "４", "５", "６", "７", "８", "９" };
		public static int Index(int rank, int file) => rank + file * 9;
		
		public static string PrettyRank(int rank) => PrettyRankTable[rank];
		
		public static string PrettyFile(int file) => PrettyFileTable[file];
	}
}