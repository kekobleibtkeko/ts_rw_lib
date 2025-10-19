using Verse;

namespace TS_Lib.Util;

public static partial class TSUtil
{
	public static int TicksGame => Find.TickManager.TicksGame;

	public class Ticks
	{
		public const int TICKS_PER_SECOND = 60;
	}
}