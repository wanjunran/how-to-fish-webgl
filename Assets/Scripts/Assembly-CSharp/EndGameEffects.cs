using UnityEngine;

public class EndGameEffects : MonoBehaviour
{
	private static EndGameEffects _instance;

	[SerializeField]
	private GameObject _endGameStuff;

	[SerializeField]
	private SelfDrivingBoat _boat;

	[Space]
	[SerializeField]
	private GameObject[] _characterHolders;

	[SerializeField]
	private GameObject[] _characters;

	[SerializeField]
	private GameObject[] _beanGuys;

	[SerializeField]
	private SkinnedMeshRenderer[] _bodyRenderers;

	[SerializeField]
	private SkinnedMeshRenderer[] _leftHands;

	[SerializeField]
	private SkinnedMeshRenderer[] _rightHands;

	[Space]
	[SerializeField]
	private SkinnedMeshRenderer[] _outfitRenderers;

	[SerializeField]
	private SkinnedMeshRenderer[] _hatRenderers;

	[SerializeField]
	private SkinnedMeshRenderer[] _accessoryRenderers;

	public static bool IsShowingEndGame { get; private set; }

	private void Awake()
	{
		_instance = this;
	}

	public static void OnShowCredits()
	{
		if (!_instance)
		{
			return;
		}
		_instance._endGameStuff.SetActive(value: true);
		_instance._boat.ToggleBoat(to: true);
		ShaderManager.ToggleSunset(sunset: true);
		MusicManager.PlayMusic("Outro", "", 0f, loop: false);
		IsShowingEndGame = true;
		for (int i = 0; i < 4; i++)
		{
			if (PlayerManager.Players.Count <= i)
			{
				_instance._characterHolders[i].SetActive(value: false);
				continue;
			}
			_instance._characterHolders[i].SetActive(value: true);
			Player player = PlayerManager.Players[i];
			PlayerSkin skin = player.Skin;
			_instance._characters[i].SetActive(!player.IsBean);
			_instance._beanGuys[i].SetActive(player.IsBean);
			if (!player.IsBean)
			{
				ShaderManager.SetPlayerColors(_instance._bodyRenderers[i], skin.SkinColor);
				ShaderManager.SetPlayerColors(_instance._leftHands[i], skin.SkinColor);
				ShaderManager.SetPlayerColors(_instance._rightHands[i], skin.SkinColor);
				_instance._hatRenderers[i].sharedMesh = SkinManager.GetHat(skin.HatMeshIndex);
				ShaderManager.SetPlayerColors(_instance._hatRenderers[i], skin.SkinColor, skin.HatColor, skin.HatColor2, skin.HatColor3);
				_instance._accessoryRenderers[i].sharedMesh = SkinManager.GetAccessory(skin.AccessoryMeshIndex);
				ShaderManager.SetPlayerColors(_instance._accessoryRenderers[i], skin.SkinColor, skin.AccessoryColor, skin.AccessoryColor2, skin.AccessoryColor3);
				_instance._outfitRenderers[i].sharedMesh = SkinManager.GetOutfit(skin.OutfitMeshIndex);
				ShaderManager.SetPlayerColors(_instance._outfitRenderers[i], skin.SkinColor, skin.OutfitColor, skin.OutfitColor2, skin.OutfitColor3);
			}
		}
	}

	public static void OnCreditsStopping()
	{
		_instance._boat.ToggleCamFollowBoat(to: false);
	}

	public static void OnHideCredits()
	{
		if ((bool)_instance && IsShowingEndGame)
		{
			IsShowingEndGame = false;
			_instance._endGameStuff.SetActive(value: false);
			_instance._boat.ToggleBoat(to: false);
			ShaderManager.ToggleSunset(sunset: false);
			MusicManager.StopMusic("Outro");
		}
	}
}
