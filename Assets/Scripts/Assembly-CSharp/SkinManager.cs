using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
	private static SkinManager _instance;

	[SerializeField]
	private Clothes[] _allClothes;

	[SerializeField]
	private Clothes _lighthouseKeeper;

	[SerializeField]
	private Clothes _swampMan;

	[SerializeField]
	private Clothes _swampLady;

	[SerializeField]
	private Clothes _kioskLady;

	[SerializeField]
	private Clothes _tourist;

	[SerializeField]
	private Clothes _grillmaster;

	[SerializeField]
	private Clothes _andrei;

	[SerializeField]
	private Clothes _jacob;

	[SerializeField]
	private Clothes _gunStoreClerc;

	[SerializeField]
	private Clothes _scaredGuyInShorts;

	[SerializeField]
	private Clothes _storeGrandma;

	[SerializeField]
	private Clothes _military;

	[SerializeField]
	private Clothes _scientist;

	private static List<OutfitMeshInfo> _allHats = new List<OutfitMeshInfo>();

	private static List<OutfitMeshInfo> _allOutfits = new List<OutfitMeshInfo>();

	private static List<OutfitMeshInfo> _allAccessories = new List<OutfitMeshInfo>();

	public static bool[] SeenHats { get; private set; }

	public static bool[] SeenOutfits { get; private set; }

	public static bool[] SeenAccesories { get; private set; }

	public static bool HasBean { get; private set; }

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
		_allHats.Add(new OutfitMeshInfo(null, unlocked: true));
		_allAccessories.Add(new OutfitMeshInfo(null, unlocked: true));
		Clothes[] allClothes = _allClothes;
		foreach (Clothes clothes in allClothes)
		{
			if ((bool)clothes.HatMesh && !MeshAlreadyInList(_allHats, clothes.HatMesh))
			{
				_allHats.Add(new OutfitMeshInfo(clothes.HatMesh, clothes.DefaultUnlocked));
			}
			if ((bool)clothes.AccessoryMesh && !MeshAlreadyInList(_allAccessories, clothes.AccessoryMesh))
			{
				_allAccessories.Add(new OutfitMeshInfo(clothes.AccessoryMesh, clothes.DefaultUnlocked));
			}
			if ((bool)clothes.OutfitMesh && !MeshAlreadyInList(_allOutfits, clothes.OutfitMesh))
			{
				_allOutfits.Add(new OutfitMeshInfo(clothes.OutfitMesh, clothes.DefaultUnlocked));
			}
		}
		if (SeenHats == null)
		{
			SeenHats = new bool[_allHats.Count];
		}
		if (SeenOutfits == null)
		{
			SeenOutfits = new bool[_allOutfits.Count];
		}
		if (SeenAccesories == null)
		{
			SeenAccesories = new bool[_allAccessories.Count];
		}
	}

	private void Start()
	{
	}

	public static void SetSeenClothes(bool[] savedSeenHats, bool[] savedSeenOutfits, bool[] savedSeenAccessories)
	{
		if (savedSeenHats != null)
		{
			for (int i = 0; i < SeenHats.Length; i++)
			{
				if (savedSeenHats.Length > i)
				{
					SeenHats[i] = savedSeenHats[i];
				}
			}
		}
		if (savedSeenOutfits != null)
		{
			for (int j = 0; j < SeenOutfits.Length; j++)
			{
				if (savedSeenOutfits.Length > j)
				{
					SeenOutfits[j] = savedSeenOutfits[j];
				}
			}
		}
		if (savedSeenAccessories == null)
		{
			return;
		}
		for (int k = 0; k < SeenAccesories.Length; k++)
		{
			if (savedSeenAccessories.Length > k)
			{
				SeenAccesories[k] = savedSeenAccessories[k];
			}
		}
	}

	public static void OnNewOutfitSelected(byte index, PlayerSkinType type)
	{
		switch (type)
		{
		case PlayerSkinType.Accessory:
		case PlayerSkinType.Accessory2:
		case PlayerSkinType.Accessory3:
			SeenAccesories[index] = true;
			break;
		case PlayerSkinType.Outfit:
		case PlayerSkinType.Outfit2:
		case PlayerSkinType.Outfit3:
			SeenOutfits[index] = true;
			break;
		case PlayerSkinType.Hat:
		case PlayerSkinType.Hat2:
		case PlayerSkinType.Hat3:
			SeenHats[index] = true;
			break;
		}
	}

	public static bool HasUnSeenSpecificClothing(PlayerSkinType type)
	{
		switch (type)
		{
		case PlayerSkinType.Accessory:
		case PlayerSkinType.Accessory2:
		case PlayerSkinType.Accessory3:
		{
			for (int j = 0; j < SeenAccesories.Length; j++)
			{
				if (!SeenAccesories[j] && _allAccessories[j].Unlocked)
				{
					return true;
				}
			}
			break;
		}
		case PlayerSkinType.Outfit:
		case PlayerSkinType.Outfit2:
		case PlayerSkinType.Outfit3:
		{
			for (int k = 0; k < SeenOutfits.Length; k++)
			{
				if (!SeenOutfits[k] && _allOutfits[k].Unlocked)
				{
					return true;
				}
			}
			break;
		}
		case PlayerSkinType.Hat:
		case PlayerSkinType.Hat2:
		case PlayerSkinType.Hat3:
		{
			for (int i = 0; i < SeenHats.Length; i++)
			{
				if (!SeenHats[i] && _allHats[i].Unlocked)
				{
					return true;
				}
			}
			break;
		}
		}
		return false;
	}

	public static bool HasUnseenClothing()
	{
		for (int i = 0; i < SeenAccesories.Length; i++)
		{
			if (!SeenAccesories[i] && _allAccessories[i].Unlocked)
			{
				return true;
			}
		}
		for (int j = 0; j < SeenOutfits.Length; j++)
		{
			if (!SeenOutfits[j] && _allOutfits[j].Unlocked)
			{
				return true;
			}
		}
		for (int k = 0; k < SeenHats.Length; k++)
		{
			if (!SeenHats[k] && _allHats[k].Unlocked)
			{
				return true;
			}
		}
		return false;
	}

	private bool MeshAlreadyInList(List<OutfitMeshInfo> list, Mesh mesh)
	{
		foreach (OutfitMeshInfo item in list)
		{
			if (item.Mesh == mesh)
			{
				return true;
			}
		}
		return false;
	}

	public static Mesh GetHat(byte value)
	{
		if ((bool)_instance)
		{
			return _allHats[Mathf.Clamp(value, 0, _allHats.Count)].Mesh;
		}
		return null;
	}

	public static Mesh GetOutfit(byte value)
	{
		if ((bool)_instance)
		{
			return _allOutfits[Mathf.Clamp(value, 0, _allOutfits.Count)].Mesh;
		}
		return null;
	}

	public static Mesh GetAccessory(byte value)
	{
		if ((bool)_instance)
		{
			return _allAccessories[Mathf.Clamp(value, 0, _allAccessories.Count)].Mesh;
		}
		return null;
	}

	public static int GetMeshIndex(PlayerSkinType type, int indexOfAll, int toChange)
	{
		if (!_instance)
		{
			return 0;
		}
		int num = 0;
		switch (type)
		{
		case PlayerSkinType.Outfit:
		case PlayerSkinType.Outfit2:
		case PlayerSkinType.Outfit3:
		{
			List<OutfitMeshInfo> list3 = new List<OutfitMeshInfo>();
			foreach (OutfitMeshInfo allOutfit in _allOutfits)
			{
				if (allOutfit.Unlocked)
				{
					list3.Add(allOutfit);
				}
			}
			num = list3.IndexOf(_allOutfits[indexOfAll]);
			num += toChange;
			if (num < 0)
			{
				num = list3.Count - 1;
			}
			else if (num >= list3.Count)
			{
				num = 0;
			}
			return _allOutfits.IndexOf(list3[num]);
		}
		case PlayerSkinType.Hat:
		case PlayerSkinType.Hat2:
		case PlayerSkinType.Hat3:
		{
			List<OutfitMeshInfo> list2 = new List<OutfitMeshInfo>();
			foreach (OutfitMeshInfo allHat in _allHats)
			{
				if (allHat.Unlocked)
				{
					list2.Add(allHat);
				}
			}
			num = list2.IndexOf(_allHats[indexOfAll]);
			num += toChange;
			if (num < 0)
			{
				num = list2.Count - 1;
			}
			else if (num >= list2.Count)
			{
				num = 0;
			}
			return _allHats.IndexOf(list2[num]);
		}
		case PlayerSkinType.Accessory:
		case PlayerSkinType.Accessory2:
		case PlayerSkinType.Accessory3:
		{
			List<OutfitMeshInfo> list = new List<OutfitMeshInfo>();
			foreach (OutfitMeshInfo allAccessory in _allAccessories)
			{
				if (allAccessory.Unlocked)
				{
					list.Add(allAccessory);
				}
			}
			num = list.IndexOf(_allAccessories[indexOfAll]);
			num += toChange;
			if (num < 0)
			{
				num = list.Count - 1;
			}
			else if (num >= list.Count)
			{
				num = 0;
			}
			return _allAccessories.IndexOf(list[num]);
		}
		default:
			return 0;
		}
	}

	private static int GetIndexOf(List<OutfitMeshInfo> list, Mesh mesh)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Mesh == mesh)
			{
				return i;
			}
		}
		return 0;
	}

	public static void UnlockLighthouseKeeper()
	{
		UnlockOutfit(_instance._lighthouseKeeper);
	}

	public static void UnlockSwampMan()
	{
		UnlockOutfit(_instance._swampMan);
	}

	public static void UnlockSwampLady()
	{
		UnlockOutfit(_instance._swampLady);
	}

	public static void UnlockKioskLady()
	{
		UnlockOutfit(_instance._kioskLady);
	}

	public static void UnlockTourist()
	{
		UnlockOutfit(_instance._tourist);
	}

	public static void UnlockGrillmaster()
	{
		UnlockOutfit(_instance._grillmaster);
	}

	public static void UnlockAndrei()
	{
		UnlockOutfit(_instance._andrei);
	}

	public static void UnlockJacob()
	{
		UnlockOutfit(_instance._jacob);
	}

	public static void UnlockGunStoreClerc()
	{
		UnlockOutfit(_instance._gunStoreClerc);
	}

	public static void UnlockScaredGuyInShorts()
	{
		UnlockOutfit(_instance._scaredGuyInShorts);
	}

	public static void UnlockStoreGradma()
	{
		UnlockOutfit(_instance._storeGrandma);
	}

	public static void UnlockMilitary()
	{
		UnlockOutfit(_instance._military);
	}

	public static void UnlockScientist()
	{
		UnlockOutfit(_instance._scientist);
	}

	public static void UnlockBean()
	{
		HasBean = true;
		ButtonManager.ShowBeanRow();
	}

	private static void UnlockOutfit(Clothes clothes)
	{
		if ((bool)clothes.HatMesh)
		{
			foreach (OutfitMeshInfo allHat in _allHats)
			{
				if (allHat.Mesh == clothes.HatMesh)
				{
					allHat.Unlock();
					break;
				}
			}
		}
		if ((bool)clothes.OutfitMesh)
		{
			foreach (OutfitMeshInfo allOutfit in _allOutfits)
			{
				if (allOutfit.Mesh == clothes.OutfitMesh)
				{
					allOutfit.Unlock();
					break;
				}
			}
		}
		if (!clothes.AccessoryMesh)
		{
			return;
		}
		foreach (OutfitMeshInfo allAccessory in _allAccessories)
		{
			if (allAccessory.Mesh == clothes.AccessoryMesh)
			{
				allAccessory.Unlock();
				break;
			}
		}
	}
}
