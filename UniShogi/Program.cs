using UniShogi;

var logger = new ConsoleLog();
var writer = new StringWriter();
var engine = new SimpleEngine();

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

	// コマンドを処理
	var result = await engine.ProcessUsiCommand(command);
	
	// 結果を出力
	logger.Log(result);
	
	// 結果を標準出力に出力
	writer.Write(result);
}

