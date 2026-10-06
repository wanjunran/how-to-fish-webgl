using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skin Preset", menuName = "How to Fish/Skin Preset")]
public class SkinPreset : ScriptableObject
{
	[SerializeField]
	private List<ItemSkin> _skins;

	public IReadOnlyList<ItemSkin> Skins => _skins.AsReadOnly();

	public void AddSkin(ItemSkin skin)
	{
		for (int i = 0; i < _skins.Count; i++)
		{
			if (!(_skins[i].Name != skin.Name))
			{
				_skins[i] = skin;
				return;
			}
		}
		_skins.Add(skin);
	}

	public byte GetRandomSkinIndex(Rarity rarity)
	{
		switch (rarity)
		{
		case Rarity.Default:
		{
			ItemSkin item3 = _skins.First((ItemSkin x) => x.Rarity == Rarity.Default);
			return (byte)_skins.IndexOf(item3);
		}
		case Rarity.Common:
		{
			List<ItemSkin> list2 = _skins.Where((ItemSkin x) => x.Rarity == Rarity.Common).ToList();
			ItemSkin item2 = list2[Random.Range(0, list2.Count)];
			return (byte)_skins.IndexOf(item2);
		}
		case Rarity.Rare:
		{
			List<ItemSkin> list3 = _skins.Where((ItemSkin x) => x.Rarity == Rarity.Rare).ToList();
			ItemSkin item4 = list3[Random.Range(0, list3.Count)];
			return (byte)_skins.IndexOf(item4);
		}
		case Rarity.Legendary:
		{
			List<ItemSkin> list = _skins.Where((ItemSkin x) => x.Rarity == Rarity.Legendary).ToList();
			ItemSkin item = list[Random.Range(0, list.Count)];
			return (byte)_skins.IndexOf(item);
		}
		default:
			return 0;
		}
	}
}
