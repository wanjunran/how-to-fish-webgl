using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Tutorial", menuName = "How to Fish/Tutorial")]
public class Tutorial : ScriptableObject
{
	[Header("Tutorial")]
	[SerializeField]
	private LocalizedString _tutorialTitleLocalized;

	[SerializeField]
	private LocalizedString _tutorialDescriptionLocalized;

	[SerializeField]
	private LocalizedString _onFinishedTitleLocalized;

	[SerializeField]
	private LocalizedString _onFinishedDescriptionLocalized;

	[SerializeField]
	private InputActionReference[] _inputActions;

	[Header("On Finished")]
	[SerializeField]
	private Tutorial nextTutorial;

	[Header("Requirements")]
	[SerializeField]
	[Tooltip("Type of event to trigger on")]
	private TutorialType tutorialType;

	public LocalizedString TutorialTitle => _tutorialTitleLocalized;

	public LocalizedString TutorialDescription => _tutorialDescriptionLocalized;

	public LocalizedString OnFinishedTitle => _onFinishedTitleLocalized;

	public LocalizedString OnFinishedDescription => _onFinishedDescriptionLocalized;

	public InputActionReference[] InputActions => _inputActions;

	public Tutorial NextTutorial => nextTutorial;

	public void Subscribe()
	{
		switch (tutorialType)
		{
		case TutorialType.OnItemSold:
			MoneyManager.OnItemSold += TutorialFinished;
			break;
		case TutorialType.OnAddedInventory:
			PlayerInventory.OnAddedToInventory += TutorialFinished;
			break;
		case TutorialType.OnItemBought:
			ItemPurchasable.OnItemBought += TutorialFinished;
			break;
		case TutorialType.OnCreatureDeath:
			Creature.OnCreatureDeath += TutorialFinished;
			break;
		case TutorialType.OnHookItem:
			Item.OnHookItem += TutorialFinished;
			break;
		case TutorialType.OnPickUpItem:
			Item.OnPickUpItem += TutorialFinished;
			break;
		case TutorialType.OnMovement:
			PlayerMovement.OnMoved += TutorialFinished;
			break;
		case TutorialType.OnRotated:
			PlayerCamera.OnPlayerRotated += TutorialFinished;
			break;
		case TutorialType.OnCreatureEaten:
			PlayerEating.OnCreatureEaten += TutorialFinished;
			break;
		case TutorialType.OnTalkedWithNPC:
			NPC.OnTalkedWithNPC += TutorialFinished;
			break;
		case TutorialType.OnPressedTab:
			PlayerThinking.OnThinking += TutorialFinished;
			break;
		case TutorialType.OnInspectedCreature:
			Creature.OnInspectedCreature += TutorialFinished;
			break;
		}
	}

	private void TutorialFinished()
	{
		TutorialManager.Instance.RemoveTutorial(this);
		Unsubscribe();
	}

	public void Unsubscribe()
	{
		switch (tutorialType)
		{
		case TutorialType.OnItemSold:
			MoneyManager.OnItemSold -= TutorialFinished;
			break;
		case TutorialType.OnAddedInventory:
			PlayerInventory.OnAddedToInventory -= TutorialFinished;
			break;
		case TutorialType.OnItemBought:
			ItemPurchasable.OnItemBought -= TutorialFinished;
			break;
		case TutorialType.OnCreatureDeath:
			Creature.OnCreatureDeath -= TutorialFinished;
			break;
		case TutorialType.OnHookItem:
			Item.OnHookItem -= TutorialFinished;
			break;
		case TutorialType.OnPickUpItem:
			Item.OnPickUpItem -= TutorialFinished;
			break;
		case TutorialType.OnMovement:
			PlayerMovement.OnMoved -= TutorialFinished;
			break;
		case TutorialType.OnRotated:
			PlayerCamera.OnPlayerRotated -= TutorialFinished;
			break;
		case TutorialType.OnCreatureEaten:
			PlayerEating.OnCreatureEaten -= TutorialFinished;
			break;
		case TutorialType.OnTalkedWithNPC:
			NPC.OnTalkedWithNPC -= TutorialFinished;
			break;
		case TutorialType.OnPressedTab:
			PlayerThinking.OnThinking -= TutorialFinished;
			break;
		case TutorialType.OnInspectedCreature:
			Creature.OnInspectedCreature -= TutorialFinished;
			break;
		}
	}
}
