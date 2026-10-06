using TMPro;
using UnityEngine;

public class FishingUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _caughtFishHolder;

	[SerializeField]
	private DripText _caughtFishDripText;

	[SerializeField]
	private TextMeshProUGUI _caughtFishText;

	[Header("Bait")]
	[SerializeField]
	private TextMeshProUGUI _baitDiffText;

	[SerializeField]
	private float _baitTextUpPos = 30f;

	[SerializeField]
	private float _baitTextMoveTime = 0.25f;

	[SerializeField]
	private float _baitTextLifeTime = 1f;

	[SerializeField]
	private float _randBaitTextPos = 25f;

	[SerializeField]
	private float _randBaitTextRotMulti = 2.5f;

	[SerializeField]
	private RectTransform _fishDiscoveredText;

	[SerializeField]
	private TextMeshProUGUI _newFishNameText;

	[SerializeField]
	private DripText _newFishDripText;

	private Vector3 _caughtFishPos;

	private Vector3 _baitTextStartPos;

	private void Awake()
	{
		_baitTextStartPos = _baitDiffText.transform.localPosition;
	}

	private void LateUpdate()
	{
		if ((bool)GameInfo.CurCamera)
		{
			Vector3 position = GameInfo.CurCamera.WorldToScreenPoint(_caughtFishPos);
			_caughtFishText.transform.position = position;
			_caughtFishText.color = ((position.z > 0f) ? Color.white : Color.clear);
		}
	}

	public void CaughtFishUI(Vector3 pos, bool isDrip)
	{
		_caughtFishPos = pos;
		_caughtFishDripText.SetDrip(isDrip);
		_caughtFishHolder.transform.position = GameInfo.CurCamera.WorldToScreenPoint(pos);
		_caughtFishHolder.transform.localScale = Vector3.zero;
		LeanTween.cancel(_caughtFishHolder);
		LeanTween.scale(_caughtFishHolder, Vector3.one, 0.4f).setEase(LeanTweenType.easeOutQuad).setOnComplete(HideFishText);
	}

	private void HideFishText()
	{
		LeanTween.scale(_caughtFishHolder, Vector3.zero, 0.8f).setEase(LeanTweenType.easeInQuad);
	}

	public void OnBaitChange(int amount, bool increased)
	{
		_baitDiffText.text = (increased ? $"+{amount}" : $"-{amount}");
		_baitDiffText.color = (increased ? GameInfo.GreenColor : GameInfo.RedColor);
		LeanTween.cancel(_baitDiffText.gameObject);
		Vector3 vector = Random.insideUnitSphere * Random.Range(0f - _randBaitTextPos, _randBaitTextPos);
		Vector3 vector2 = _baitTextStartPos + vector;
		_baitDiffText.transform.localPosition = vector2;
		_baitDiffText.transform.localScale = Vector3.one;
		_baitDiffText.transform.rotation = Quaternion.Euler(0f, 0f, (0f - vector.x) * _randBaitTextRotMulti);
		LeanTween.moveLocal(_baitDiffText.gameObject, vector2 + _baitDiffText.transform.up * _baitTextUpPos, _baitTextMoveTime).setEase(LeanTweenType.easeOutBack);
		LeanTween.value(_baitDiffText.gameObject, 0f, 1f, _baitTextLifeTime).setOnComplete(RemoveBaitText);
	}

	private void RemoveBaitText()
	{
		LeanTween.cancel(_baitDiffText.gameObject);
		LeanTween.scale(_baitDiffText.gameObject, Vector3.zero, _baitTextMoveTime).setEase(LeanTweenType.easeInBack);
	}

	public void OnNewFishCaught(Creature creature)
	{
		LeanTween.cancel(_fishDiscoveredText.gameObject);
		_fishDiscoveredText.localScale = Vector3.zero;
		LeanTween.scale(_fishDiscoveredText.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutBack);
		LeanTween.value(_fishDiscoveredText.gameObject, 0f, 1f, 1f).setOnComplete(ShowNewFishName);
		AudioManager.PlayGlobalClip("PlayerSpawn", variation: true);
		string text = (creature.IsDrip ? (LocalizationManager.DripLocalized.GetLocalizedString() + " ") : "");
		_newFishDripText.SetDrip(creature.IsDrip);
		_newFishNameText.text = text + creature.GetName();
	}

	private void ShowNewFishName()
	{
		LeanTween.cancel(_newFishNameText.gameObject);
		LeanTween.scale(_newFishNameText.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutBack);
		LeanTween.value(_newFishNameText.gameObject, 0f, 1f, 3f).setOnComplete(RemoveNewFishCaughtUI);
	}

	private void RemoveNewFishCaughtUI()
	{
		LeanTween.scale(_fishDiscoveredText, Vector3.zero, 0.25f).setEase(LeanTweenType.easeInBack);
		LeanTween.scale(_newFishNameText.gameObject, Vector3.zero, 0.25f).setEase(LeanTweenType.easeInBack);
	}
}
