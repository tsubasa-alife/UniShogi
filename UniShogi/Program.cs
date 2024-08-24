using UniShogi;

var logger = new ConsoleLog();
var engine = new EngineEntryPoint();

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
	
	engine.ReceiveUsiCommand(command);
}

