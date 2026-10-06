using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JournalSlot : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _nameText;

	[SerializeField]
	private TextMeshProUGUI _worthText;

	[SerializeField]
	protected MeshFilter _filter;

	[SerializeField]
	private GameObject _slotToScale;

	[SerializeField]
	private RawImage _image;

	[SerializeField]
	private DripText _dripText;

	public void SetCreature(Creature creature)
	{
		if ((bool)creature)
		{
			SavedCreature savedCreature = SaveManager.GetSavedCreature(creature);
			_image.color = (savedCreature.BeenKilled ? Color.white : Color.black);
			bool flag = creature.BossType != BossType.None || savedCreature.KilledDrip;
			_nameText.text = (savedCreature.BeenKilled ? creature.GetName() : "???");
			_worthText.text = ((savedCreature.BeenKilled && creature.BossType != BossType.Boss) ? $"${creature.DefaultWorth}" : "-");
			_nameText.gameObject.SetActive(value: true);
			_dripText.SetDrip(savedCreature.BeenKilled & flag);
			_filter.mesh = ((flag && creature.BossType == BossType.None) ? creature.DripMesh : creature.Mesh);
			_filter.transform.localPosition = creature.InventoryMeshPos;
			_filter.transform.localEulerAngles = creature.InventoryMeshRot;
			_filter.transform.localScale = Vector3.one * creature.InventoryMeshScale;
		}
		else
		{
			_nameText.text = "";
			_nameText.gameObject.SetActive(value: false);
			_worthText.text = "";
			_filter.mesh = null;
		}
		_slotToScale.transform.localScale = Vector3.zero;
		LeanTween.cancel(_slotToScale);
	}

	public void ShowSlot()
	{
		LeanTween.cancel(_slotToScale);
		LeanTween.scale(_slotToScale, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutBack);
	}
}
