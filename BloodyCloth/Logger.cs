using System;

namespace BloodyCloth;

public class Logger
{
    private readonly DateTime _startDate = DateTime.Now;
    private readonly string _name;

    public string Name => _name;

    public Logger(string name = "main")
    {
        this._name = name;
    }

    public void LogInfo(object message)
    {
        var date = DateTime.Now - _startDate;
        Console.WriteLine($"[{date.Hours}:{date.Minutes}:{date.Seconds}] [{Name}/INFO] {message ?? "null"}");
    }

    public void LogError(object message)
    {
        var date = DateTime.Now - _startDate;
        Console.WriteLine($"[{date.Hours}:{date.Minutes}:{date.Seconds}] [{Name}/ERROR] {message ?? "null"}");
    }

    public void LogWarning(object message)
    {
        var date = DateTime.Now - _startDate;
        Console.WriteLine($"[{date.Hours}:{date.Minutes}:{date.Seconds}] [{Name}/WARN] {message}");
    }
}
