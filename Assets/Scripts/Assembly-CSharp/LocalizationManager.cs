using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizationManager : MonoBehaviour
{
	private static LocalizationManager _instance;

	[SerializeField]
	private LocalizedString _dripLocalized;

	[SerializeField]
	private LocalizedString _unequipItemLocalized;

	[SerializeField]
	private LocalizedString _alreadyBoughtLocalized;

	[SerializeField]
	private LocalizedString _talkLocalized;

	[SerializeField]
	private LocalizedString _cantAttachLocalized;

	[SerializeField]
	private LocalizedString _equipWeaponLocalized;

	[SerializeField]
	private LocalizedString _equipMeleeWeaponLocalized;

	[SerializeField]
	private LocalizedString _worthLocalized;

	[SerializeField]
	private LocalizedString _cookingLocalized;

	[SerializeField]
	private LocalizedString _upgradeSharpnessLocalized;

	[SerializeField]
	private LocalizedString _sharpnessDescriptionLocalized;

	[SerializeField]
	private LocalizedString _maxedOnIslandLocalized;

	[SerializeField]
	private LocalizedString _upgradeBulletsLocalized;

	[SerializeField]
	private LocalizedString _bulletDescriptionLocalized;

	[SerializeField]
	private LocalizedString _extraInventorySlotLocalized;

	[SerializeField]
	private LocalizedString _takeLocalized;

	[SerializeField]
	private LocalizedString _lighterLocalized;

	[SerializeField]
	private LocalizedString _usbForIslandLocalized;

	[SerializeField]
	private LocalizedString _boatKeysLocalized;

	[SerializeField]
	private LocalizedString _damageLocalized;

	[SerializeField]
	private LocalizedString _sharpnessLocalized;

	[SerializeField]
	private LocalizedString _ammoLocalized;

	[SerializeField]
	private LocalizedString _attachmentsLocalized;

	[SerializeField]
	private LocalizedString _questItemLocalized;

	[SerializeField]
	private LocalizedString _driveLocalized;

	[SerializeField]
	private LocalizedString _cantDriveLocalized;

	[SerializeField]
	private LocalizedString _noKeysLocalized;

	[SerializeField]
	private LocalizedString _getBackLocalized;

	[SerializeField]
	private LocalizedString _creaturesLocalized;

	[SerializeField]
	private LocalizedString _dripCreaturesLocalized;

	[SerializeField]
	private LocalizedString _isRollingLocalized;

	[SerializeField]
	private LocalizedString _prizesLocalized;

	[SerializeField]
	private LocalizedString _weightLocalized;

	[SerializeField]
	private LocalizedString _savedLocalized;

	[SerializeField]
	private LocalizedString _endangeredSpeciesLocalized;

	[SerializeField]
	private LocalizedString _respawnLocalized;

	[SerializeField]
	private LocalizedString _enterTextLocalized;

	[SerializeField]
	private LocalizedString _boatLocalized;

	[SerializeField]
	private LocalizedString _kilogramLocalized;

	[SerializeField]
	private LocalizedString _closeLocalized;

	[Header("KIll score translations")]
	[SerializeField]
	private LocalizedString _killedLocalized;

	[SerializeField]
	private LocalizedString _killscoreLocalized;

	[SerializeField]
	private LocalizedString _360Localized;

	[SerializeField]
	private LocalizedString _noScopeLocalized;

	[SerializeField]
	private LocalizedString _quickScopeLocalized;

	[SerializeField]
	private LocalizedString _longshotLocalized;

	[SerializeField]
	private LocalizedString _headshotLocalized;

	[SerializeField]
	private LocalizedString _dogfightLocalized;

	[SerializeField]
	private LocalizedString _aerialLocalized;

	[SerializeField]
	private LocalizedString _flyFishingLocalized;

	[SerializeField]
	private LocalizedString _noobKillLocalized;

	[SerializeField]
	private LocalizedString _oneShotOneKillLocalized;

	[SerializeField]
	private LocalizedString _doubleKillLocalized;

	[SerializeField]
	private LocalizedString _tripleKillLocalized;

	[SerializeField]
	private LocalizedString _quadraKillLocalized;

	[SerializeField]
	private LocalizedString _pentaKillLocalized;

	[SerializeField]
	private LocalizedString _multikillLocalized;

	[SerializeField]
	private LocalizedString _finallyLocalized;

	[SerializeField]
	private LocalizedString _endangeredLocalized;

	[SerializeField]
	private LocalizedString _killstealLocalized;

	[SerializeField]
	private LocalizedString _pointBlankLocalized;

	[SerializeField]
	private LocalizedString _overkillLocalized;

	[SerializeField]
	private LocalizedString _lastBulletLocalized;

	[SerializeField]
	private LocalizedString _impressiveLocalized;

	[SerializeField]
	private LocalizedString _placeItemsLocalized;

	[SerializeField]
	private LocalizedString _blackLocalized;

	[SerializeField]
	private LocalizedString _redLocalized;

	[SerializeField]
	private LocalizedString _greenLocalized;

	[SerializeField]
	private LocalizedString _youLostLocalized;

	[SerializeField]
	private LocalizedString _betOnLocalized;

	[SerializeField]
	private LocalizedString _meleeLocalized;

	[SerializeField]
	private LocalizedString _explosionLocalized;

	[Header("Rebind translations")]
	[SerializeField]
	private LocalizedString _upLocalized;

	[SerializeField]
	private LocalizedString _leftLocalized;

	[SerializeField]
	private LocalizedString _rightLocalized;

	[SerializeField]
	private LocalizedString _downLocalized;

	[SerializeField]
	private LocalizedString _negativeLocalized;

	[SerializeField]
	private LocalizedString _positiveLocalized;

	[SerializeField]
	private LocalizedString _waitingForInputLocalized;

	[SerializeField]
	private LocalizedString _bindingLocalized;

	[SerializeField]
	private LocalizedString _scrollDownLocalized;

	[SerializeField]
	private LocalizedString _scrollUpLocalized;

	[SerializeField]
	private LocalizedString _scrollLocalized;

	[SerializeField]
	private LocalizedString _hoursLocalized;

	[SerializeField]
	private LocalizedString _minutesLocalized;

	[SerializeField]
	private LocalizedString _secondsLocalized;

	[Header("Held Item Infos")]
	[SerializeField]
	private HeldInfo _heldCreatureInfo;

	[SerializeField]
	private HeldInfo _heldToolInfo;

	[SerializeField]
	private HeldInfo _heldDeadPlayerInfo;

	[Header("Default Font Assets")]
	[SerializeField]
	private TMP_FontAsset _defaultFontAsset;

	[SerializeField]
	private TMP_FontAsset _backdropFontAsset;

	[Header("Fallback Assets")]
	[SerializeField]
	private TMP_FontAsset _simplifiedFont;

	[SerializeField]
	private TMP_FontAsset _simplifiedBDFont;

	[Space]
	[SerializeField]
	private TMP_FontAsset _traditionalFont;

	[SerializeField]
	private TMP_FontAsset _traditionalBDFont;

	[Space]
	[SerializeField]
	private TMP_FontAsset _japaneseFont;

	[SerializeField]
	private TMP_FontAsset _japaneseBDFont;

	[Space]
	[SerializeField]
	private TMP_FontAsset _koreanFont;

	[SerializeField]
	private TMP_FontAsset _koreanBDFont;

	private static List<Locale> _locales;

	private static bool _initialized;

	public static LocalizedString DripLocalized => _instance._dripLocalized;

	public static LocalizedString UnequipItemLocalized => _instance._unequipItemLocalized;

	public static LocalizedString AlreadyBoughtLocalized => _instance._alreadyBoughtLocalized;

	public static LocalizedString TalkLocalized => _instance._talkLocalized;

	public static LocalizedString CantAttachLocalized => _instance._cantAttachLocalized;

	public static LocalizedString EquipWeaponLocalized => _instance._equipWeaponLocalized;

	public static LocalizedString EquipMeleeWeaponLocalizedLocalized => _instance._equipMeleeWeaponLocalized;

	public static LocalizedString WorthLocalized => _instance._worthLocalized;

	public static LocalizedString CookingLocalized => _instance._cookingLocalized;

	public static LocalizedString UpgradeSharpnessLocalized => _instance._upgradeSharpnessLocalized;

	public static LocalizedString SharpnessDescriptionLocalized => _instance._sharpnessDescriptionLocalized;

	public static LocalizedString MaxedOnIslandLocalized => _instance._maxedOnIslandLocalized;

	public static LocalizedString UpgradeBulletsLocalized => _instance._upgradeBulletsLocalized;

	public static LocalizedString BulletDescriptionLocalized => _instance._bulletDescriptionLocalized;

	public static LocalizedString ExtraInventorySlotLocalized => _instance._extraInventorySlotLocalized;

	public static LocalizedString TakeLocalized => _instance._takeLocalized;

	public static LocalizedString LighterLocalized => _instance._lighterLocalized;

	public static LocalizedString UsbForIslandLocalized => _instance._usbForIslandLocalized;

	public static LocalizedString BoatKeysLocalized => _instance._boatKeysLocalized;

	public static LocalizedString DamageLocalized => _instance._damageLocalized;

	public static LocalizedString SharpnessLocalized => _instance._sharpnessLocalized;

	public static LocalizedString AmmoLocalized => _instance._ammoLocalized;

	public static LocalizedString AttachmentsLocalized => _instance._attachmentsLocalized;

	public static LocalizedString QuestItemLocalized => _instance._questItemLocalized;

	public static LocalizedString DriveLocalized => _instance._driveLocalized;

	public static LocalizedString CantDriveLocalized => _instance._cantDriveLocalized;

	public static LocalizedString NoKeysLocalized => _instance._noKeysLocalized;

	public static LocalizedString GetBackLocalized => _instance._getBackLocalized;

	public static LocalizedString CreaturesLocalized => _instance._creaturesLocalized;

	public static LocalizedString DripCreaturesLocalized => _instance._dripCreaturesLocalized;

	public static LocalizedString IsRollingLocalized => _instance._isRollingLocalized;

	public static LocalizedString PrizesLocalized => _instance._prizesLocalized;

	public static LocalizedString WeightLocalized => _instance._weightLocalized;

	public static LocalizedString SavedLocalized => _instance._savedLocalized;

	public static LocalizedString EndangeredSpeciesLocalized => _instance._endangeredSpeciesLocalized;

	public static LocalizedString RespawnLocalized => _instance._respawnLocalized;

	public static LocalizedString EnterTextLocalized => _instance._enterTextLocalized;

	public static LocalizedString BoatLocalized => _instance._boatLocalized;

	public static LocalizedString KilogramLocalized => _instance._kilogramLocalized;

	public static LocalizedString CloseLocalized => _instance._closeLocalized;

	public static LocalizedString KilledLocalized => _instance._killedLocalized;

	public static LocalizedString KillscoreLocalized => _instance._killscoreLocalized;

	public static LocalizedString ThreeSixtyLocalized => _instance._360Localized;

	public static LocalizedString NoScopeLocalized => _instance._noScopeLocalized;

	public static LocalizedString QuickScopeLocalized => _instance._quickScopeLocalized;

	public static LocalizedString LongshotLocalized => _instance._longshotLocalized;

	public static LocalizedString HeadshotLocalized => _instance._headshotLocalized;

	public static LocalizedString DogfightLocalized => _instance._dogfightLocalized;

	public static LocalizedString AerialLocalized => _instance._aerialLocalized;

	public static LocalizedString FlyFishingLocalized => _instance._flyFishingLocalized;

	public static LocalizedString NoobKillLocalized => _instance._noobKillLocalized;

	public static LocalizedString OneShotOneKillLocalized => _instance._oneShotOneKillLocalized;

	public static LocalizedString DoubleKillLocalized => _instance._doubleKillLocalized;

	public static LocalizedString TripleKillLocalized => _instance._tripleKillLocalized;

	public static LocalizedString QuadraKillLocalized => _instance._quadraKillLocalized;

	public static LocalizedString PentaKillLocalized => _instance._pentaKillLocalized;

	public static LocalizedString MultikillLocalized => _instance._multikillLocalized;

	public static LocalizedString FinallyLocalized => _instance._finallyLocalized;

	public static LocalizedString EndangeredLocalized => _instance._endangeredLocalized;

	public static LocalizedString KillstealLocalized => _instance._killstealLocalized;

	public static LocalizedString PointBlankLocalized => _instance._pointBlankLocalized;

	public static LocalizedString OverkillLocalized => _instance._overkillLocalized;

	public static LocalizedString LastBulletLocalized => _instance._lastBulletLocalized;

	public static LocalizedString ImpressiveLocalized => _instance._impressiveLocalized;

	public static LocalizedString MeleeLocalized => _instance._meleeLocalized;

	public static LocalizedString ExplosionLocalized => _instance._explosionLocalized;

	public static LocalizedString UpLocalized => _instance._upLocalized;

	public static LocalizedString LeftLocalized => _instance._leftLocalized;

	public static LocalizedString RightLocalized => _instance._rightLocalized;

	public static LocalizedString DownLocalized => _instance._downLocalized;

	public static LocalizedString NegativeLocalized => _instance._negativeLocalized;

	public static LocalizedString PositiveLocalized => _instance._positiveLocalized;

	public static LocalizedString WaitingForInputLocalized => _instance._waitingForInputLocalized;

	public static LocalizedString BindingLocalized => _instance._bindingLocalized;

	public static LocalizedString ScrollDownLocalized => _instance._scrollDownLocalized;

	public static LocalizedString ScrollUpLocalized => _instance._scrollUpLocalized;

	public static LocalizedString ScrollLocalized => _instance._scrollLocalized;

	public static LocalizedString HoursLocalized => _instance._hoursLocalized;

	public static LocalizedString MinutesLocalized => _instance._minutesLocalized;

	public static LocalizedString SecondsLocalized => _instance._secondsLocalized;

	public static LocalizedString PlaceItemsLocalized => _instance._placeItemsLocalized;

	public static LocalizedString BlackLocalized => _instance._blackLocalized;

	public static LocalizedString RedLocalized => _instance._redLocalized;

	public static LocalizedString GreenLocalized => _instance._greenLocalized;

	public static LocalizedString YouLostLocalized => _instance._youLostLocalized;

	public static LocalizedString BetOnLocalized => _instance._betOnLocalized;

	public static HeldInfo HeldCreatureInfo => _instance._heldCreatureInfo;

	public static HeldInfo HeldToolInfo => _instance._heldToolInfo;

	public static HeldInfo HeldDeadPlayerInfo => _instance._heldDeadPlayerInfo;

	public static bool IsAsianLanguage { get; private set; }

	public static int CurLanguage { get; private set; }

	private void Awake()
	{
		_instance = this;
		_locales = LocalizationSettings.AvailableLocales.Locales;
	}

	private void Start()
	{
		StartCoroutine(WaitForLocalization());
	}

	private IEnumerator WaitForLocalization()
	{
		yield return new WaitUntil(() => LocalizationSettings.InitializationOperation.IsDone);
		if (!_initialized)
		{
			SetToSteamLanguage();
		}
		else
		{
			SetLanguage(CurLanguage);
		}
	}

	private void SetToSteamLanguage()
	{
		int language = 0;
		switch (SteamUtils.GetSteamUILanguage())
		{
		case "english":
			language = 0;
			break;
		case "swedish":
			language = 1;
			break;
		case "schinese":
			language = 2;
			break;
		case "tchinese":
			language = 3;
			break;
		case "french":
			language = 4;
			break;
		case "german":
			language = 5;
			break;
		case "italian":
			language = 6;
			break;
		case "japanese":
			language = 7;
			break;
		case "koreana":
			language = 8;
			break;
		case "polish":
			language = 9;
			break;
		case "brazilian":
			language = 10;
			break;
		case "russian":
			language = 11;
			break;
		case "latam":
			language = 12;
			break;
		case "spanish":
			language = 13;
			break;
		case "turkish":
			language = 14;
			break;
		case "ukrainian":
			language = 15;
			break;
		}
		SetLanguage(language);
	}

	public static void ChangeLanguage(bool prev = false)
	{
		int curLanguage = CurLanguage;
		curLanguage += ((!prev) ? 1 : (-1));
		if (curLanguage < 0)
		{
			curLanguage = _locales.Count - 1;
		}
		else if (curLanguage >= _locales.Count)
		{
			curLanguage = 0;
		}
		SetLanguage(curLanguage);
	}

	public static void SetLanguage(int toIndex)
	{
		_initialized = true;
		CurLanguage = Mathf.Clamp(toIndex, 0, _locales.Count - 1);
		if (LocalizationSettings.InitializationOperation.IsDone)
		{
			LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[CurLanguage];
			_instance.UpdateFallbackFonts();
		}
	}

	private void UpdateFallbackFonts()
	{
		_defaultFontAsset.fallbackFontAssetTable.Clear();
		_backdropFontAsset.fallbackFontAssetTable.Clear();
		IsAsianLanguage = true;
		switch (CurLanguage)
		{
		case 2:
			_defaultFontAsset.fallbackFontAssetTable.Add(_simplifiedFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_traditionalFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_japaneseFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_koreanFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_simplifiedBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_traditionalBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_japaneseBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_koreanBDFont);
			break;
		case 3:
			_defaultFontAsset.fallbackFontAssetTable.Add(_traditionalFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_simplifiedFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_japaneseFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_koreanFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_traditionalBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_simplifiedBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_japaneseBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_koreanBDFont);
			break;
		case 7:
			_defaultFontAsset.fallbackFontAssetTable.Add(_japaneseFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_simplifiedFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_traditionalFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_koreanFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_japaneseBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_simplifiedBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_traditionalBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_koreanBDFont);
			break;
		case 8:
			_defaultFontAsset.fallbackFontAssetTable.Add(_koreanFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_simplifiedFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_traditionalFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_japaneseFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_koreanBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_simplifiedBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_traditionalBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_japaneseBDFont);
			break;
		default:
			IsAsianLanguage = false;
			_defaultFontAsset.fallbackFontAssetTable.Add(_simplifiedFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_traditionalFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_japaneseFont);
			_defaultFontAsset.fallbackFontAssetTable.Add(_koreanFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_simplifiedBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_traditionalBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_japaneseBDFont);
			_backdropFontAsset.fallbackFontAssetTable.Add(_koreanBDFont);
			break;
		}
	}
}
