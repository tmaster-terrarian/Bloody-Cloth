using System;

namespace BloodyCloth;

public class Logger
{
    private static readonly DateTime _startDate = DateTime.Now;
    private readonly string _name;

    public string Name => _name;

    public Logger(string name = "main")
    {
        this._name = name;
    }

    public void LogInfo(object message) => _Log("INFO", message);

    public void LogError(object message) => _Log("ERROR", message);

    public void LogWarning(object message) => _Log("WARN", message);

    private void _Log(string type, object message)
    {
        var date = DateTime.Now - _startDate;
        Console.WriteLine(VerifyString($"[{date.Hours}:{date.Minutes}:{date.Seconds}] [{Name}/{type}] {message}"));
    }

    internal static string VerifyString(string str)
    {
        // TODO: make a method that converts unsafe strings into safe ones
        return str;
    }
}
