namespace UniShogi
{
	/// <summary>
	/// 将棋エンジンの起点となるクラス
	/// </summary>
	public class BaseEngine
	{
		protected ILogger _logger;
		protected Position _position;
		
		public void Initialize(ILogger logger)
		{
			_logger = logger;
		}

		protected virtual void Usi(){}

		protected virtual void IsReady(){}

		protected virtual void UsiNewGame(){}

		protected virtual void SetOption(){}
		
		protected virtual void Position(){}

		protected virtual void Go(){}

		protected virtual void Stop(){}

		protected virtual void Quit(){}

		public void ProcessUsiCommand(string command)
		{
			switch (command)
			{
				case "usi":
					Usi();
					_logger.Log("usiok");
					break;
				case "setoption":
					SetOption();
					break;
				case "isready":
					IsReady();
					_logger.Log("readyok");
					break;
				case "usinewgame":
					UsiNewGame();
					break;
				case "position":
					Position();
					break;
				case "go":
					Go();
					break;
				case "stop":
					Stop();
					break;
				case "quit":
					Quit();
					break;
				default:
					_logger.Log($"info string Unsupported command: command={command}");
					break;
			}
		}
		
	}
}