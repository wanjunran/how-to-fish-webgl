using UnityEngine;

public class ClientSettings : MonoBehaviour
{
	public static bool CheatsEnabled { get; private set; }

	public static void ToggleCheats(bool to)
	{
		CheatsEnabled = to;
	}
}
