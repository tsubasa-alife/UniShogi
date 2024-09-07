using Cysharp.Threading.Tasks;

namespace UniShogi
{
	/// <summary>
	/// 合法手からランダムに手を選ぶエンジン
	/// </summary>
	public class SimpleEngine : BaseEngine
	{
		protected override UniTask<string> Usi()
		{
			_logger.Log("id name UniShogi");
			_logger.Log("id author Tsubasa Hizono");
			return UniTask.FromResult("usiok");
		}

		protected override UniTask<string> IsReady()
		{
			return UniTask.FromResult("readyok");
		}

		protected override UniTask<string> UsiNewGame()
		{
			return UniTask.FromResult("usinewgame");
		}

		/// <summary>
		/// 局面を受け取る
		/// </summary>
		protected override UniTask<string> Position()
		{
			return UniTask.FromResult("position");
		}

		protected override UniTask<string> Go()
		{
			return UniTask.FromResult("go");
		}

		protected override UniTask<string> Stop()
		{
			return UniTask.FromResult("stop");
		}

		protected override UniTask<string> Quit()
		{
			return UniTask.FromResult("quit");
		}
	}
}