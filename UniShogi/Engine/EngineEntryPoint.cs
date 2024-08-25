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