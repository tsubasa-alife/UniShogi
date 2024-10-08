/*
 * Copyright (c) 2022 tomori-k
 */

namespace UniShogi
{
	/// <summary>
	/// 先後
	/// </summary>
	public enum Color
	{
		Black, White
	}

	/// <summary>
	/// Color の拡張メソッドを定義するクラス
	/// </summary>
	public static class ColorExtensions
	{
		/// <summary>
		/// c の先後を反転させた Color を作成
		/// </summary>
		public static Color Opponent(this Color c)
		{
			return (Color)((int)c ^ 1);
		}

		public static string Usi(this Color c)
		{
			return c == Color.Black ? "b" : "w";
		}

		public static string Csa(this Color c)
		{
			return c == Color.Black ? "+" : "-";
		}

		public enum PrettyType
		{
			/// <summary>
			/// 先手：▲ 
			/// 後手：△
			/// </summary>
			Triangle,

			/// <summary>
			/// 先手："先手"
			/// 後手："後手"
			/// </summary>
			SenteGote,
		}

		/// <summary>
		/// 人が読みやすい文字列に変換
		/// </summary>
		public static string Pretty(this Color c, PrettyType type = PrettyType.SenteGote)
		{
			switch (type)
			{
				case PrettyType.Triangle:
					return c == Color.Black ? "▲" : "△";
				case PrettyType.SenteGote:
					return c == Color.Black ? "先手" : "後手";
				default:
					throw new ArgumentException($"変換タイプが不正です：{type}");
			}
		}
	}
}