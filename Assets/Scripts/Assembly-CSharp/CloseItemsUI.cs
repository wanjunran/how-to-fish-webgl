using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseItemsUI : MonoBehaviour
{
	private const float ClosestItemsRefreshInterval = 0.1f;

	[SerializeField]
	private Transform _itemDotsCanvas;

	[SerializeField]
	private Image _dotPrefab;

	[SerializeField]
	private Color _aliveColor = Color.indianRed;

	[SerializeField]
	[Min(0f)]
	private int _closeItemsAmount = 5;

	private List<Image> _closeItemsDots = new List<Image>();

	private HashSet<Item> _uniqueItems = new HashSet<Item>();

	private Item[] _closestItems;

	private float[] _closestItemSqrDistances;

	private float _nextClosestItemsRefreshTime;

	private static bool _dotsEnabled;

	public static void ToggleItemDots(bool to)
	{
		_dotsEnabled = to;
	}

	public void InitializeLocal()
	{
		int num = Mathf.Max(0, _closeItemsAmount);
		_closestItems = new Item[num];
		_closestItemSqrDistances = new float[num];
		for (int i = _closeItemsDots.Count; i < num; i++)
		{
			Image image = Object.Instantiate(_dotPrefab, _itemDotsCanvas);
			image.color = Color.clear;
			_closeItemsDots.Add(image);
		}
		HideDots();
		RefreshClosestItems();
	}

	private void LateUpdate()
	{
		if (!Player.LocalPlayer || !GameInfo.CurCamera || !_dotsEnabled)
		{
			HideDots();
			return;
		}
		if (Time.unscaledTime >= _nextClosestItemsRefreshTime)
		{
			RefreshClosestItems();
		}
		Camera curCamera = GameInfo.CurCamera;
		int num = Mathf.Min(_closeItemsDots.Count, _closestItems.Length);
		for (int i = 0; i < num; i++)
		{
			Item item = _closestItems[i];
			if (ShouldHideItem(item))
			{
				_closeItemsDots[i].color = Color.clear;
				continue;
			}
			Vector3 position = curCamera.WorldToScreenPoint(item.transform.position);
			bool flag = position.z > 0f && position.x >= 0f && position.x <= (float)curCamera.pixelWidth && position.y >= 0f && position.y <= (float)curCamera.pixelHeight;
			bool num2 = (bool)item.Creature && !item.Creature.IsDead;
			bool flag2 = item.DeadPlayer;
			Color color = (num2 ? _aliveColor : (flag2 ? GameInfo.OrangeColor : Color.white));
			_closeItemsDots[i].color = (flag ? color : Color.clear);
			if (flag)
			{
				_closeItemsDots[i].transform.position = position;
			}
		}
		for (int j = num; j < _closeItemsDots.Count; j++)
		{
			_closeItemsDots[j].color = Color.clear;
		}
	}

	private void RefreshClosestItems()
	{
		_nextClosestItemsRefreshTime = Time.unscaledTime + 0.1f;
		for (int i = 0; i < _closestItems.Length; i++)
		{
			_closestItems[i] = null;
			_closestItemSqrDistances[i] = float.MaxValue;
		}
		if (!Player.LocalPlayer)
		{
			return;
		}
		_uniqueItems.Clear();
		Vector3 position = Player.LocalPlayer.Transform.position;
		foreach (KeyValuePair<Transform, Item> item2 in ItemManager.Items)
		{
			Item value = item2.Value;
			if (ShouldHideItem(value) || !_uniqueItems.Add(value))
			{
				continue;
			}
			float sqrMagnitude = (value.transform.position - position).sqrMagnitude;
			for (int j = 0; j < _closestItems.Length; j++)
			{
				Item item = _closestItems[j];
				bool num = value.Creature;
				bool flag = (bool)item && (bool)item.Creature;
				bool flag2 = !num & flag;
				bool flag3 = num == flag;
				if (!item || flag2 || (flag3 && !(sqrMagnitude >= _closestItemSqrDistances[j])))
				{
					for (int num2 = _closestItems.Length - 1; num2 > j; num2--)
					{
						_closestItems[num2] = _closestItems[num2 - 1];
						_closestItemSqrDistances[num2] = _closestItemSqrDistances[num2 - 1];
					}
					_closestItems[j] = value;
					_closestItemSqrDistances[j] = sqrMagnitude;
					break;
				}
			}
		}
	}

	private static bool ShouldHideItem(Item item)
	{
		if (!item || (bool)item.Holder || item.IsDeinitializing)
		{
			return true;
		}
		if ((bool)item.Bird && !item.Bird.IsDead)
		{
			return true;
		}
		return item.IgnoredByCloseDots;
	}

	private void HideDots()
	{
		for (int i = 0; i < _closeItemsDots.Count; i++)
		{
			_closeItemsDots[i].color = Color.clear;
		}
	}
}
