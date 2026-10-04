using System.Linq;
using UnityEngine;

public class ClientCommands : MonoBehaviour
{
	private const string InvertX = "invertx";

	private const string InvertY = "inverty";

	public static bool IsClientCommand(string fullCommand)
	{
		if (string.IsNullOrEmpty(fullCommand))
		{
			return false;
		}
		if (!fullCommand.StartsWith("!"))
		{
			return false;
		}
		string[] array = fullCommand.Split(' ');
		string text = array[0];
		string text2 = "";
		for (int i = 1; i < array.Length; i++)
		{
			text2 += array[i];
		}
		text = text.Remove(0, 1).ToLower();
		string[] array2 = array;
		if (array2.Length > 1)
		{
			array2 = array2.Skip(1).ToArray();
		}
		if (text == "invertx")
		{
			InvertMouse(x: true);
		}
		else if (text == "inverty")
		{
			InvertMouse(x: false);
		}
		else
		{
			ChatManager.ChatMessage("Couldn't find command called <b>" + fullCommand + "</b>");
		}
		return true;
	}

	private static void InvertMouse(bool x)
	{
		SaveManager.ToggleInvertedMouse(x);
	}
}
