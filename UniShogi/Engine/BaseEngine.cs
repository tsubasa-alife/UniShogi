using System.Text;
using Cysharp.Threading.Tasks;

namespace UniShogi
{
	/// <summary>
	/// USIプロトコルによる将棋エンジン用クラス
	/// </summary>
	public class BaseEngine
	{
		protected ILogger _logger;
		protected Position _position;
		protected Random _random;
		
		public void Initialize(ILogger logger)
		{
			_logger = logger;
			_position = new Position();
			_random = new Random();
		}

		protected virtual UniTask<string> Usi()
		{
			return UniTask.FromResult(string.Empty);
		}

		protected virtual UniTask<string> IsReady()
		{
			return UniTask.FromResult(string.Empty);
		}

		protected virtual UniTask<string> UsiNewGame()
		{
			return UniTask.FromResult(string.Empty);
		}

		protected virtual UniTask<string> SetOption()
		{
			return UniTask.FromResult(string.Empty);
		}

		protected virtual UniTask<string> Position(string[] command)
		{
			return UniTask.FromResult(string.Empty);
		}

		protected virtual UniTask<string> Go()
		{
			return UniTask.FromResult(string.Empty);
		}

		protected virtual UniTask<string> Stop()
		{
			return UniTask.FromResult(string.Empty);
		}

		protected virtual UniTask<string> Quit()
		{
			return UniTask.FromResult(string.Empty);
		}

		public async UniTask<string> ProcessUsiCommand(string[] command)
		{
			switch (command[0])
			{
				case "usi":
					return await Usi();
				case "setoption":
					return await SetOption();
				case "isready":
					return await IsReady();
				case "usinewgame":
					return await UsiNewGame();
				case "position":
					return await Position(command);
				case "go":
					return await Go();
				case "stop":
					return await Stop();
				case "quit":
					return await Quit();
				default:
					return $"info string Unsupported command: command={command}";
			}
		}
		
	}
}