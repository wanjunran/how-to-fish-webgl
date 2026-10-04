using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Interactable : MonoBehaviour
{
	[FormerlySerializedAs("_modelToOutline")]
	[SerializeField]
	protected GameObject[] _modelsToOutline;

	[SerializeField]
	private Collider _interactCol;

	[SerializeField]
	private Transform _textTarget;

	private static Dictionary<Collider, Interactable> _colToInteractables = new Dictionary<Collider, Interactable>();

	protected bool _isHovering;

	public virtual bool InteractableWhenHoldingItem => true;

	public Transform TextTarget => _textTarget;

	public event Action<Player> OnInteract;

	protected virtual void Awake()
	{
		_colToInteractables.Add(_interactCol, this);
	}

	public void ToggleIsInteractable(bool to)
	{
		base.gameObject.layer = LayerMask.NameToLayer(to ? "Interactable" : "Default");
	}

	protected virtual void OnDestroy()
	{
		_colToInteractables.Remove(_interactCol);
	}

	public virtual void Hover()
	{
		_isHovering = true;
		GameObject[] modelsToOutline = _modelsToOutline;
		for (int i = 0; i < modelsToOutline.Length; i++)
		{
			modelsToOutline[i].layer = LayerMask.NameToLayer("OutlinedObject");
		}
	}

	public virtual void UnHover()
	{
		_isHovering = false;
		GameObject[] modelsToOutline = _modelsToOutline;
		for (int i = 0; i < modelsToOutline.Length; i++)
		{
			modelsToOutline[i].layer = LayerMask.NameToLayer("Default");
		}
	}

	public virtual void Interact(Player player)
	{
		OnInteract?.Invoke(player);
	}

	public static Interactable Get(Collider col)
	{
		return _colToInteractables.GetValueOrDefault(col);
	}
}
