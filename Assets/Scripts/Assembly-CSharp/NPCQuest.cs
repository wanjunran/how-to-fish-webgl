using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New NPC Quest", menuName = "How to Fish/NPC Quest")]
public class NPCQuest : ScriptableObject
{
	[SerializeField]
	private List<Item> _questItems;

	[SerializeField]
	private bool _onlyCreatures;

	[SerializeField]
	private byte _totalItems = 1;

	[SerializeField]
	private Mesh _interactableMesh;

	[SerializeField]
	private BaitInfo _baitToReceive;

	[SerializeField]
	private QuestType _type;

	[Header("If unlock island")]
	[SerializeField]
	[Tooltip("First island = 1, not starting from 0")]
	private byte _islandToUnlock;

	[Space]
	[Header("Localized things to say")]
	[SerializeField]
	private LocalizedString[] _linesLocalized;

	[SerializeField]
	private LocalizedString[] _onItemReceivedLinesLocalized;

	[SerializeField]
	private LocalizedString[] _onQuestCompletedLinesLocalized;

	[SerializeField]
	private LocalizedString[] _holdingItemLinesLocalized;

	[SerializeField]
	private LocalizedString[] _alreadyCompletedLinesLocalized;

	public LocalizedString[] Lines => _linesLocalized;

	public LocalizedString[] OnItemReceivedLines => _onItemReceivedLinesLocalized;

	public LocalizedString[] OnQuestCompletedLines => _onQuestCompletedLinesLocalized;

	public LocalizedString[] HoldingItemLines => _holdingItemLinesLocalized;

	public LocalizedString[] AlreadyCompletedLines => _alreadyCompletedLinesLocalized;

	public byte IslandToUnlock => _islandToUnlock;

	public List<Item> QuestItems => _questItems;

	public bool OnlyCreatures => _onlyCreatures;

	public byte TotalItems => _totalItems;

	public Mesh InteractableMesh => _interactableMesh;

	public BaitInfo BaitToReceive => _baitToReceive;

	public QuestType Type => _type;

	public bool HasItem(Item toCheck)
	{
		foreach (Item questItem in _questItems)
		{
			if (questItem.ID == toCheck.ID)
			{
				return true;
			}
		}
		return false;
	}
}
