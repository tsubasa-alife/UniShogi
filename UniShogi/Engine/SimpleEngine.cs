using Cysharp.Threading.Tasks;
using System;

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
		protected override UniTask<string> Position(string[] command)
		{
			_position.Set(command[1]);
			return UniTask.FromResult(string.Empty);
		}

		protected override UniTask<string> Go()
		{
			// 局面から合法手を一つ選びそれを返す
			var moves = Movegen.GenerateMoves(_position);
			var move = moves[_random.Next(moves.Count)];
			return UniTask.FromResult($"bestmove {move.ToUsi()}");
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