using Verse;

namespace TS_Lib.Util;

public static partial class TSUtil
{
	public static (TaggedString, TaggedString) LabelDesc(string key, TranslatorDelegate? translator = null, params NamedArgument[] args)
	{
		translator ??= (key, args) => key.Translate(args);
		return (
			translator($"{key}.label", args),
			translator($"{key}.desc", args)
		);
	}
	public static (TaggedString, TaggedString) LabelDesc(string key, SimpleTranslatorDelegate? translator = null)
	{
		translator ??= (key) => key.Translate();
		return (
			translator($"{key}.label"),
			translator($"{key}.desc")
		);
	}
}