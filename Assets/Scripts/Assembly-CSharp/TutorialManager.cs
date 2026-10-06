using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
	public static TutorialManager Instance;

	[SerializeField]
	private Tutorial startingTutorial;

	private readonly HashSet<Tutorial> _activeTutorials = new HashSet<Tutorial>();

	public IReadOnlyCollection<Tutorial> ActiveTutorials => _activeTutorials;

	private void Awake()
	{
		Setter.SetSingleInstance(ref Instance, this);
	}

	public void Start()
	{
		if ((bool)startingTutorial)
		{
			AddTutorial(startingTutorial);
		}
	}

	private void OnDisable()
	{
		foreach (Tutorial activeTutorial in _activeTutorials)
		{
			activeTutorial.Unsubscribe();
		}
		_activeTutorials.Clear();
	}

	private void AddTutorial(Tutorial tutorial)
	{
		if (!_activeTutorials.Contains(tutorial))
		{
			_activeTutorials.Add(tutorial);
			tutorial.Subscribe();
			if ((bool)PlayerTutorial.Instance)
			{
				PlayerTutorial.Instance.AddTutorial(tutorial);
			}
		}
	}

	public void RemoveTutorial(Tutorial tutorial)
	{
		_activeTutorials.Remove(tutorial);
		if ((bool)tutorial.NextTutorial)
		{
			AddTutorial(tutorial.NextTutorial);
		}
		else if ((bool)Player.LocalPlayer)
		{
			Server.Instance.SendFinishedTutorial(Player.LocalPlayer);
		}
		if ((bool)PlayerTutorial.Instance)
		{
			PlayerTutorial.Instance.FinishTutorial(tutorial);
		}
	}
}
