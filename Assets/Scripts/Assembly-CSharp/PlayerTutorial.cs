using System.Collections.Generic;
using UnityEngine;

public class PlayerTutorial : MonoBehaviour
{
	public static PlayerTutorial Instance;

	[SerializeField]
	private RectTransform _tutorialHolder;

	[SerializeField]
	private CanvasGroup _tutorialGroup;

	[SerializeField]
	private TutorialBox _box;

	private Dictionary<Tutorial, TutorialBox> _tutorialToBox = new Dictionary<Tutorial, TutorialBox>();

	private Queue<KeyValuePair<Tutorial, bool>> _tutorialQueue = new Queue<KeyValuePair<Tutorial, bool>>();

	private bool _isFocusedOnQuest;

	private Player _player;

	private bool _hide;

	public void InitializeLocal(Player player)
	{
		Setter.SetSingleInstance(ref Instance, this);
		_player = player;
		if (!TutorialManager.Instance)
		{
			return;
		}
		foreach (Tutorial activeTutorial in TutorialManager.Instance.ActiveTutorials)
		{
			AddTutorial(activeTutorial);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Comma) && ClientSettings.CheatsEnabled)
		{
			_hide = !_hide;
			if ((bool)_tutorialGroup)
			{
				_tutorialGroup.alpha = ((!_hide) ? 1 : 0);
			}
		}
	}

	public void AddTutorial(Tutorial tutorial)
	{
		if (!_tutorialToBox.ContainsKey(tutorial))
		{
			TutorialBox tutorialBox = Object.Instantiate(_box, _tutorialHolder);
			tutorialBox.Initialize(tutorial, _tutorialHolder);
			_tutorialToBox.Add(tutorial, tutorialBox);
			OnQuestEvent(tutorial, finished: false);
		}
	}

	private void OnQuestEvent(Tutorial newTutorial, bool finished)
	{
		_tutorialQueue.Enqueue(new KeyValuePair<Tutorial, bool>(newTutorial, finished));
	}

	public void FinishTutorial(Tutorial tutorial)
	{
		if (_tutorialToBox.ContainsKey(tutorial))
		{
			_tutorialToBox[tutorial].Finish();
			_tutorialToBox.Remove(tutorial);
			OnQuestEvent(tutorial, finished: true);
		}
	}

	public void SkipTutorial()
	{
		foreach (KeyValuePair<Tutorial, TutorialBox> item in _tutorialToBox)
		{
			item.Key.Unsubscribe();
			Object.Destroy(item.Value.gameObject);
		}
		_tutorialToBox.Clear();
	}
}
