using System;
using UnityEngine.Serialization;

[Serializable]
public class SavedCreature
{
	public byte ID;

	[FormerlySerializedAs("BeenCaught")]
	public bool BeenKilled;

	[FormerlySerializedAs("CaughtShiny")]
	public bool KilledDrip;
}
