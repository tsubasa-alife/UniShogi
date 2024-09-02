namespace UniShogi
{
	/// <summary>
	/// 将棋エンジンの起点となるクラス
	/// </summary>
	public class EngineEntryPoint
	{
		public const string USI_Ponder = "USI_Ponder";
		public const string USI_Hash = "USI_Hash";
		public const string EvalDir = "EvalDir";
		public const string BookFile = "BookFile";
		public const string IgnoreBookPlay = "IgnoreBookPlay";

		private ILogger _logger;
		
		public void Initialize(ILogger logger)
		{
			_logger = logger;
			Setup();
		}

		private void Setup()
		{
			var pos = new Position("lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1"); // 平手

			_logger.Log(pos.Sfen());

			_logger.Log(pos.Pretty());
			pos.DoMove(Usi.ParseMove("2g2f"));
			_logger.Log(pos.Pretty());

			_logger.Log(pos.Player == Color.White); // true

			pos.UndoMove();
			
			_logger.Log(pos.InCheck()); // false
			_logger.Log(pos.IsMated()); // false
			_logger.Log(pos.IsLegalMove(Usi.ParseMove("7g7f"))); // true
			_logger.Log(pos.CheckRepetition() == Repetition.None); // true
			_logger.Log(pos.CanDeclareWin()); // false

			foreach (var m in Movegen.GenerateMoves(pos))
			{
				_logger.Log(m.ToUsi());
			}
			
			_logger.Log("Setup Completed.");
		}

		public void ReceiveUsiCommand(string command)
		{
			switch (command)
			{
				case "usi":
					_logger.Log("id name UniShogi");
					_logger.Log("id author Tsubasa Hizono");
					_logger.Log($"option name {USI_Ponder} type check default true");
					_logger.Log($"option name {USI_Hash} type spin default 256");
					_logger.Log($"option name {EvalDir} type string default eval");
					_logger.Log($"option name {BookFile} type string default user_book1.db");
					_logger.Log($"option name {IgnoreBookPlay} type check default true");
					_logger.Log("usiok");
					break;
				default:
					_logger.Log($"info string Unsupported command: command={command}");
					break;
			}
		}
		
	}
}