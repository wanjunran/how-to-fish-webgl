using TMPro;
using UnityEngine;

public class CreditsTitle : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _titleText;

	private void OnEnable()
	{
		ToUpperFirstLetter();
	}

	private void ToUpperFirstLetter()
	{
		if (string.IsNullOrEmpty(_titleText.text))
		{
			return;
		}
		char[] array = _titleText.text.ToLower().ToCharArray();
		bool flag = true;
		for (int i = 0; i < array.Length; i++)
		{
			if (char.IsWhiteSpace(array[i]))
			{
				flag = true;
			}
			else if (flag)
			{
				array[i] = char.ToUpper(array[i]);
				flag = false;
			}
		}
		_titleText.text = new string(array);
	}
}
