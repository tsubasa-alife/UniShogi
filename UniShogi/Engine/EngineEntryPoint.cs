namespace UniShogi
{
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
					break;
				default:
					_logger.Log($"info string Unsupported command: command={command}");
					break;
			}
		}
		
	}
}