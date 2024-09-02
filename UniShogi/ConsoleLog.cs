using UniShogi;

public class ConsoleLog : ILogger
{
	public void Log(object message)
	{
		Console.WriteLine(message);
	}
}