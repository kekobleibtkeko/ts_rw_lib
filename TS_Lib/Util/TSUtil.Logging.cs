using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TS_Lib.Util;

public class TSLogger(string sawmill, Color? color = null)
{
	public enum Level
	{
		Off,
		Verbose,
		Debug,
		Info,
		Warning,
		Error,
		Fatal,
	}

	public Color SawmillColor = color ?? Color.grey;
	public string Sawmill = sawmill;
	public Level ActiveLevel = Level.Info;

	private readonly HashSet<int> LoggedMessages = [];

	public TSLogger(string sawmill, Level level) : this(sawmill)
	{
		ActiveLevel = level;
	}

	public string GetSawmill() => $"[{new TSText(Sawmill).Clr(SawmillColor)}]";
	public void Log(Level level, string content, int? id = null)
	{
		if (id.HasValue && LoggedMessages.Contains(id.Value))
			return;

		if (level == Level.Off || level < ActiveLevel)
			return;
		var final_text = $"{GetSawmill()} {new TSText(content).Clr(level.ToColor())} ({level})";
		switch (level)
		{
			default:
			case Level.Off:
			case Level.Verbose:
			case Level.Debug:
			case Level.Info:
				Verse.Log.Message(final_text);
				break;
			case Level.Warning:
				Verse.Log.Warning(final_text);
				break;
			case Level.Error:
			case Level.Fatal:
				Verse.Log.Error(final_text);
				break;
		}
		if (id.HasValue)
			LoggedMessages.Add(id.Value);
	}

	public void Verbose(string content, int? id = null) => Log(Level.Verbose, content, id);
	public void Debug(string content, int? id = null) => Log(Level.Debug, content, id);
	public void Info(string content, int? id = null) => Log(Level.Info, content, id);
	public void Warning(string content, int? id = null) => Log(Level.Warning, content, id);
	public void Error(string content, int? id = null) => Log(Level.Error, content, id);
	public void Fatal(string content, int? id = null) => Log(Level.Fatal, content, id);
}

public static partial class TSUtil
{
	public static Color ToColor(this TSLogger.Level level)
	=> level switch
	{
		TSLogger.Level.Verbose => Color.gray,
		TSLogger.Level.Debug => Color.white,
		TSLogger.Level.Info => Color.cyan,
		TSLogger.Level.Warning => Color.yellow,
		TSLogger.Level.Error => Color.red,
		TSLogger.Level.Fatal => ColorFromHTML("#C300FF"),
		TSLogger.Level.Off or _ => Color.gray,
	};
}