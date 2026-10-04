using System;

[Serializable]
public struct Bonus
{
	public string Name;

	public float Worth;

	public Bonus(string name, float worth)
	{
		Name = name;
		Worth = worth;
	}
}
