using UniShogi;



var logger = new ConsoleLog();
var writer = new StringWriter();
var engine = new SimpleEngine();
var mode = Mode.Engine;

var pos = new Position("lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1");

logger.Log(pos.Pretty());

engine.Initialize(logger);

string commandLine = null;

// メインループ
while ((commandLine = Console.ReadLine()) != null)
{
	// コマンドラインをスペースで分割
	var command = commandLine.Split();
	
	// コマンドが空の場合は無視
	if (command.Length == 0)
	{
		continue;
	}

	// 新規対局を開始する
	if (command[0] == "start")
	{
		mode = Mode.Battle;
		pos = new Position("lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1");
		logger.Log("started new game");
		logger.Log(pos.Pretty());
		continue;
	}

	if (mode == Mode.Engine)
	{
		// コマンドを処理
		var result = await engine.ProcessUsiCommand(command);

		if (!String.IsNullOrEmpty(result))
		{
			// 結果を出力
			logger.Log(result);
	
			// 結果を標準出力に出力
			writer.Write(result);
		}
	}
	else
	{
		if (!Usi.TryParseMove(command[0], out var move))
		{
			logger.Log("Invalid Command");
			continue;
		}
		
		if (!pos.IsLegalMove(move))
		{
			logger.Log("Illegal Move");
			continue;
		}
		
		pos.DoMove(move);
		logger.Log(pos.Pretty());
		await engine.ProcessUsiCommand(new string[] { "position", pos.Sfen() });
		var result = await engine.ProcessUsiCommand(new string[] { "go" });
		logger.Log(result);
		var results = result.Split();
		pos.DoMove(Usi.ParseMove(results[1]));
		logger.Log(pos.Pretty());
	}

	
}

enum Mode
{
	Engine,
	Battle
}

