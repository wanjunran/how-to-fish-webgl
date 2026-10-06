using System.Collections.Generic;

internal class KillScore
{
	public string Killed;

	public int Worth;

	public List<Bonus> Bonuses;

	public KillScore(string killed, int worth, List<Bonus> bonuses)
	{
		Killed = killed;
		Worth = worth;
		Bonuses = bonuses;
	}
}
