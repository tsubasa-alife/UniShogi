using UniShogi;

var logger = new ConsoleLog();
var engine = new BaseEngine();

var pos = new Position("lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1");

logger.Log(pos.Pretty());

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

