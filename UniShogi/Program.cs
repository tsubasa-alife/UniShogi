using UniShogi;

var logger = new ConsoleLog();
var engine = new BaseEngine();

var pos = new Position("lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1"); // 平手

logger.Log(pos.Sfen());

logger.Log(pos.Pretty());

pos.DoMove(Usi.ParseMove("2g2f"));

logger.Log(pos.Pretty());

logger.Log(pos.Player == Color.White); // true

pos.UndoMove();
			
logger.Log(pos.InCheck()); // false
logger.Log(pos.IsMated()); // false
logger.Log(pos.IsLegalMove(Usi.ParseMove("7g7f"))); // true
logger.Log(pos.CheckRepetition() == Repetition.None); // true
logger.Log(pos.CanDeclareWin()); // false

foreach (var m in Movegen.GenerateMoves(pos))
{
	logger.Log(m.ToUsi());
}

logger.Log(Movegen.GenerateMoves(pos).Count); // 31
			
logger.Log("Test Completed.");

engine.Initialize(logger);

string commandLine = null;

while ((commandLine = Console.ReadLine()) != null)
{
	var split = commandLine.Split();
	if (split.Length == 0)
	{
		continue;
	}

	var command = split[0];
	
	engine.ProcessUsiCommand(command);
}

