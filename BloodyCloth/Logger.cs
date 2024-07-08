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

    public void LogInfo(object message) => _Log("INFO", message ?? "null");

    public void LogError(object message) => _Log("ERROR", message ?? "null");

    public void LogWarning(object message) => _Log("WARN", message ?? "null");

    private void _Log(string type, object message)
    {
        var date = DateTime.Now - _startDate;
        Console.WriteLine(VerifyString($"[{date.Hours.ToString("D2")}:{date.Minutes.ToString("D2")}:{date.Seconds.ToString("D2")}] [{Name}/{type}] {message}"));
    }

    internal static string VerifyString(string str)
    {
        var newStr = "";
        foreach(char ch in str)
        {
            if(Main.ValidChars.Contains(ch)) newStr += ch;
            else newStr += ' ';
        }
        return newStr;
    }
}
