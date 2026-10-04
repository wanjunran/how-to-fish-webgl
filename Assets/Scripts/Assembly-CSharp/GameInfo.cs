using System.Collections.Generic;
using System.Linq;
using FishNet;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class GameInfo : MonoBehaviour
{
	private static GameInfo _instance;

	[SerializeField]
	private PlayerInput _playerInput;

	[SerializeField]
	private Item[] _itemsWithSkinsForCommands;

	[Header("Prefabs")]
	[SerializeField]
	private Player _playerPrefab;

	[SerializeField]
	private DeadPlayer _deadPlayerPrefab;

	[SerializeField]
	private GameObject _serverPrefab;

	[SerializeField]
	private Item _cheatQuestItem;

	[Header("Layers")]
	[SerializeField]
	private LayerMask _levelLayer;

	[SerializeField]
	private LayerMask _itemLayer;

	[SerializeField]
	private LayerMask _itemPartLayer;

	[SerializeField]
	private LayerMask _itemAndInHandLayer;

	[SerializeField]
	private LayerMask _interactableLayer;

	[SerializeField]
	private LayerMask _projectileHitLayer;

	[SerializeField]
	private LayerMask _npcProjectileHitLayer;

	[SerializeField]
	private LayerMask _canJumpOnLayers;

	[SerializeField]
	private LayerMask _affectedByExplosionsLayer;

	[SerializeField]
	private LayerMask _boatLayer;

	[SerializeField]
	private LayerMask _playerLayers;

	[SerializeField]
	private LayerMask _playerLayer;

	[SerializeField]
	private LayerMask _legStepOnLayers;

	[SerializeField]
	private LayerMask _npcLayer;

	[Header("Gameplay")]
	[SerializeField]
	[Tooltip("Below this velocity, if an item is very low to the ground, it will not collide with players")]
	private float _minVelForItemsToHavePlayerColWhenLow = 3f;

	[SerializeField]
	private float _headShotDamageMulti = 1.5f;

	[FormerlySerializedAs("_projectilePlayerForce")]
	[SerializeField]
	private float _playerKillForce;

	[SerializeField]
	private float _playerDeathForceMultiFromCreature;

	[SerializeField]
	private float _boatProjectileForce;

	[SerializeField]
	private float _maxItemVel;

	[SerializeField]
	private AnimationCurve _cooknessWorthCurve;

	[SerializeField]
	private AnimationCurve _cooknessEatCurve;

	[SerializeField]
	private float _snapToServerTime = 0.25f;

	[SerializeField]
	private AnimationCurve _snapToServerCurve;

	[SerializeField]
	private WorldText _worldTextPrefab;

	[SerializeField]
	private PhysicsMaterial _holdingPhysicsMat;

	[Header("Visuals")]
	[SerializeField]
	private AnimationCurve _bloodDecalSizeMulti;

	[SerializeField]
	private Volume _globalVolume;

	[SerializeField]
	private UniversalRenderPipelineAsset _urpAsset;

	[SerializeField]
	private Light _mainLight;

	[Header("Colors")]
	[SerializeField]
	private Color _redColor;

	[SerializeField]
	private Color _greenColor;

	[SerializeField]
	private Color _orangeColor;

	[FormerlySerializedAs("_orangeColor")]
	[SerializeField]
	private Color _minBossColor;

	[SerializeField]
	private Color _commonColor;

	[SerializeField]
	private Color _rareColor;

	[SerializeField]
	private Color _legendaryColor;

	[SerializeField]
	private Gradient _dripGradient;

	[SerializeField]
	private RenderTexture _inventoryTexture;

	[SerializeField]
	private RenderTexture _journalTexture;

	[SerializeField]
	private RenderTexture _slotMachineTexture;

	[Header("Fishing")]
	[SerializeField]
	private BaitInfo[] _defaultBaits;

	[SerializeField]
	private Vector2 _defaultMinMaxFishVelForDamage;

	[SerializeField]
	private float _defaultVelDamageMulti;

	[SerializeField]
	private BaitInfo _emptyBait;

	[Header("Fishing")]
	[SerializeField]
	private string _defaultStepSound;

	[SerializeField]
	private string _waterStepSound;

	[SerializeField]
	private string _boatStepSound;

	[SerializeField]
	private int _stepSoundCount;

	public static float TickMulti;

	private static List<BaitInfo> _allBaits = new List<BaitInfo>();

	private static List<AttachmentInfo> _allAttachments = new List<AttachmentInfo>();

	private static List<Creature> _allCreatures = new List<Creature>();

	private static Dictionary<byte, Item> _allItems = new Dictionary<byte, Item>();

	private static Dictionary<string, Item> _nameToSpawnable = new Dictionary<string, Item>();

	private static Dictionary<byte, Item> _idToSpawnable = new Dictionary<byte, Item>();

	public static PlayerInput Input => _instance._playerInput;

	public static Item[] ItemWithSkinsforCommands => _instance._itemsWithSkinsForCommands;

	public static Player PlayerPrefab => _instance._playerPrefab;

	public static DeadPlayer DeadPlayerPrefab => _instance._deadPlayerPrefab;

	public static GameObject ServerPrefab => _instance._serverPrefab;

	public static Item CheatQuestItem => _instance._cheatQuestItem;

	public static LayerMask LevelLayer => _instance._levelLayer;

	public static LayerMask ItemLayer => _instance._itemLayer;

	public static LayerMask ItemPartLayer => _instance._itemPartLayer;

	public static LayerMask ItemAndInHandLayer => _instance._itemAndInHandLayer;

	public static LayerMask InteractableLayer => _instance._interactableLayer;

	public static LayerMask ProjectileHitLayer => _instance._projectileHitLayer;

	public static LayerMask NpcProjectileHitLayer => _instance._npcProjectileHitLayer;

	public static LayerMask CanJumpOnLayers => _instance._canJumpOnLayers;

	public static LayerMask AffectedByExplosionLayer => _instance._affectedByExplosionsLayer;

	public static LayerMask BoatLayer => _instance._boatLayer;

	public static LayerMask PlayerLayers => _instance._playerLayers;

	public static LayerMask PlayerLayer => _instance._playerLayer;

	public static LayerMask LegStepOnLayers => _instance._legStepOnLayers;

	public static LayerMask NpcLayer => _instance._npcLayer;

	public static float MinVelForItemsToHavePlayerColWhenLow => _instance._minVelForItemsToHavePlayerColWhenLow;

	public static float HeadShotDamageMulti => _instance._headShotDamageMulti;

	public static float PlayerKillForce => _instance._playerKillForce;

	public static float PlayerDeathForceMultiFromCreature => _instance._playerDeathForceMultiFromCreature;

	public static float BoatProjectileForce => _instance._boatProjectileForce;

	public static float MaxItemVel => _instance._maxItemVel;

	public static float FixedTimeMultiplier { get; private set; }

	public static float SnapToServerTime => _instance._snapToServerTime;

	public static AnimationCurve SnapToServerCurve => _instance._snapToServerCurve;

	public static AnimationCurve CooknessWorthCurve => _instance._cooknessWorthCurve;

	public static AnimationCurve CooknessEatCurve => _instance._cooknessEatCurve;

	public static int Seed { get; private set; }

	public static Camera CurCamera { get; private set; }

	public static WorldText WorldTextPrefab => _instance._worldTextPrefab;

	public static PhysicsMaterial HoldingPhysicsMat => _instance._holdingPhysicsMat;

	public static AnimationCurve BloodDecalSizeMulti => _instance._bloodDecalSizeMulti;

	public static Volume GlobalVolume => _instance._globalVolume;

	public static Color RedColor => _instance._redColor;

	public static Color GreenColor => _instance._greenColor;

	public static Color OrangeColor => _instance._orangeColor;

	public static Color MiniBossColor => _instance._minBossColor;

	public static Color CommonColor => _instance._commonColor;

	public static Color RareColor => _instance._rareColor;

	public static Color LegendaryColor => _instance._legendaryColor;

	public static Gradient DripGradient => _instance._dripGradient;

	public static RenderTexture InventoryTexture => _instance._inventoryTexture;

	public static RenderTexture JournalTexture => _instance._journalTexture;

	public static RenderTexture SlotMachineTexture => _instance._slotMachineTexture;

	public static Vector2 DefaultMinMaxFishVelForDamage => _instance._defaultMinMaxFishVelForDamage;

	public static float DefaultVelDamageMulti => _instance._defaultVelDamageMulti;

	public static string DefaultStepSound => _instance._defaultStepSound;

	public static string WaterStepSound => _instance._waterStepSound;

	public static string BoatStepSound => _instance._boatStepSound;

	public static int StepSoundCount => _instance._stepSoundCount;

	public static UniversalRenderPipelineAsset UrpAsset => _instance._urpAsset;

	public static IReadOnlyList<BaitInfo> AllBaits => _allBaits;

	public static IReadOnlyList<AttachmentInfo> AllAttachments => _allAttachments;

	public static Light MainLight => _instance._mainLight;

	public static int AllCreatureCount => _allCreatures.Count;

	public static byte GetIndexOfBait(BaitInfo bait)
	{
		return (byte)_allBaits.IndexOf(bait);
	}

	public static byte GetIndexOfAttachment(AttachmentInfo attachment)
	{
		return (byte)_allAttachments.IndexOf(attachment);
	}

	public static Creature GetCreature(int index)
	{
		if (index < _allCreatures.Count)
		{
			return _allCreatures[index];
		}
		return null;
	}

	public static bool HasKilledAllCreatures(bool checkDrip)
	{
		if (!SaveManager.HasLocalSave)
		{
			return false;
		}
		int num = 0;
		foreach (Creature allCreature in _allCreatures)
		{
			if (!allCreature.ExcludeFromJournal)
			{
				if (!SaveManager.HasKilledCreature(allCreature.ID, checkDrip))
				{
					return false;
				}
				num++;
			}
		}
		return true;
	}

	public static int KilledCreaturesCount(bool checkDrip)
	{
		if (!SaveManager.HasLocalSave)
		{
			return 0;
		}
		int num = 0;
		foreach (Creature allCreature in _allCreatures)
		{
			if (!allCreature.ExcludeFromJournal && SaveManager.HasKilledCreature(allCreature.ID, checkDrip))
			{
				num++;
			}
		}
		return num;
	}

	public static Item GetSpawnable(string name)
	{
		if (_nameToSpawnable.ContainsKey(name))
		{
			return _nameToSpawnable[name];
		}
		return null;
	}

	public static Item GetSpawnable(byte id)
	{
		if (_idToSpawnable.ContainsKey(id))
		{
			return _idToSpawnable[id];
		}
		return null;
	}

	public static Item IDToItem(byte id)
	{
		if (!_allItems.ContainsKey(id))
		{
			return null;
		}
		return _allItems[id];
	}

	private void Awake()
	{
		_instance = this;
		int num = Mathf.Clamp((int)((float)Screen.height * 0.25f), 480, 1440);
		int num2 = (int)((float)num * 4.40625f);
		_inventoryTexture.width = num2;
		_inventoryTexture.height = num;
		_journalTexture.width = num2;
		_journalTexture.height = num;
		_slotMachineTexture.width = num2 * 2;
		_slotMachineTexture.height = num * 2;
		FixedTimeMultiplier = 1f / Time.fixedDeltaTime;
		if ((bool)InstanceFinder.TimeManager)
		{
			TickMulti = 1f / (float)(int)InstanceFinder.TimeManager.TickRate / Time.fixedDeltaTime;
		}
		_allBaits.Add(_emptyBait);
		BaitInfo[] array = Resources.LoadAll<BaitInfo>("Baits");
		foreach (BaitInfo baitInfo in array)
		{
			if (!_defaultBaits.Contains(baitInfo))
			{
				_allBaits.Add(baitInfo);
			}
		}
		_allBaits = _allBaits.OrderBy((BaitInfo x) => x.Cost).ToList();
		AttachmentInfo[] array2 = Resources.LoadAll<AttachmentInfo>("Attachments");
		foreach (AttachmentInfo item in array2)
		{
			_allAttachments.Add(item);
		}
		_allAttachments = _allAttachments.OrderBy((AttachmentInfo x) => x.name).ToList();
		Creature[] array3 = Resources.LoadAll<Creature>("Items/Creatures");
		foreach (Creature creature in array3)
		{
			if (!creature.ExcludeFromJournal)
			{
				_allCreatures.Add(creature);
			}
		}
		_allCreatures = (from x in _allCreatures
			orderby x.BossType == BossType.None descending, x.DefaultWorth
			select x).ToList();
		Item[] array4 = Resources.LoadAll<Item>("Items");
		foreach (Item item2 in array4)
		{
			_nameToSpawnable.Add(item2.name.Replace(" ", "").ToLower(), item2);
			_idToSpawnable.TryAdd(item2.ID, item2);
			_allItems.Add(item2.ID, item2);
		}
	}

	public static void GenerateSeed()
	{
		if (SteamManager.CurrentLobbyID != CSteamID.Nil)
		{
			Seed = (int)SteamManager.CurrentLobbyID.m_SteamID;
		}
		else
		{
			Seed = Random.Range(0, int.MaxValue);
		}
	}

	public static void SetCam(Camera cam)
	{
		CurCamera = cam;
	}

	public static void ToggleAllCreaturesKilled(bool to, bool drip)
	{
		foreach (Creature allCreature in _allCreatures)
		{
			if (!allCreature.ExcludeFromJournal)
			{
				SaveManager.IsNewFish(allCreature.ID, to, drip || allCreature.BossType != BossType.None);
			}
		}
	}
}
