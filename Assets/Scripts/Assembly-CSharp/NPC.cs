using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

public class NPC : MonoBehaviour
{
	private enum MouthState
	{
		None = 0,
		Gaping = 1,
		Eating = 2,
		Speaking = 3
	}

	[SerializeField]
	[Tooltip("Don't initialize stuff if for screenshot (Won't get error when starting since it doesn't do anything)")]
	private bool _forScreenshot;

	[Header("SET ID MANUALLY TO SOMETHING UNIQUE")]
	[SerializeField]
	private byte _id;

	[Space]
	[SerializeField]
	private Interactable _characterInteractable;

	[FormerlySerializedAs("_singleUseInteractable")]
	[SerializeField]
	private QuestInteractable questInteractable;

	[SerializeField]
	private NPCInteractable _npcInteractable;

	[SerializeField]
	private List<NPCQuest> _quests;

	[Header("Skin")]
	[SerializeField]
	private Vector3 _skinColor;

	[SerializeField]
	private SkinnedMeshRenderer _head;

	[FormerlySerializedAs("_body")]
	[SerializeField]
	private SkinnedMeshRenderer _outfit;

	[SerializeField]
	private SkinnedMeshRenderer _handLeft;

	[SerializeField]
	private SkinnedMeshRenderer _handRight;

	[Header("Mouth Settings")]
	[SerializeField]
	private Transform _mouthPosForItems;

	[SerializeField]
	private Transform _mouthLower;

	[SerializeField]
	private Transform _mouthUpper;

	[SerializeField]
	private Vector2 _gapingMouthRotMinMax = new Vector2(15f, 25f);

	[SerializeField]
	private float _gapingMouthSpeed = 0.25f;

	[SerializeField]
	private float _speakingDistance = 8f;

	[SerializeField]
	private byte _speakMovements = 3;

	[Space]
	[SerializeField]
	private byte _chews = 5;

	[SerializeField]
	private float _singleChewTime = 0.075f;

	[Space]
	[SerializeField]
	private AudioSource _mouthSource;

	[SerializeField]
	private float _mouthVol = 0.75f;

	[Header("Eye Settings")]
	[SerializeField]
	private Transform[] _eyes;

	[SerializeField]
	private float _eyeRotSpeed = 15f;

	[SerializeField]
	private float _maxEyeRot = 45f;

	[FormerlySerializedAs("_playerEyeContactMaxDi")]
	[SerializeField]
	[Tooltip("Eyes will not target players further away than this")]
	private float _playerEyeContactMaxDist = 4f;

	[Header("Hand Settings")]
	[SerializeField]
	private Transform _handHoldingItem;

	[SerializeField]
	private Vector3 _holdingItemPos;

	[SerializeField]
	private Vector3 _holdingItemRot;

	private byte[] _serverProgressions;

	private Vector3 _handStartPos;

	private Vector3 _handStartEuler;

	private Vector3 _eyePos;

	private Vector3 _defaultEyeForward;

	private Quaternion _defaultEyeRot;

	private Quaternion _targetEyeRot;

	private byte _curQuestIndex;

	private Transform _targetLookingAt;

	private float _playerEyeContactMaxDistSqr;

	private float _curMouthX;

	private float _mouthStartRot;

	private MouthState _mouthState;

	private byte _speakMovementsLeft;

	private byte _curLineIndex;

	private Vector3 _shaderSkinColor;

	public Transform MouthPosForItems => _mouthPosForItems;

	public IReadOnlyList<NPCQuest> Quests => _quests;

	public byte ID => _id;

	public static event Action OnTalkedWithNPC;

	private void Awake()
	{
		if (_forScreenshot)
		{
			return;
		}
		NPCManager.AddNpc(this);
		_handStartPos = _handHoldingItem.localPosition;
		_handStartEuler = _handHoldingItem.localEulerAngles;
		_mouthStartRot = _mouthLower.localEulerAngles.x;
		_curMouthX = _mouthStartRot;
		_playerEyeContactMaxDistSqr = _playerEyeContactMaxDist * _playerEyeContactMaxDist;
		_characterInteractable.OnInteract += OnInteract;
		if (_eyes.Length != 0)
		{
			_defaultEyeRot = _eyes[0].localRotation;
			_defaultEyeForward = _eyes[0].up;
			Transform[] eyes = _eyes;
			foreach (Transform transform in eyes)
			{
				_eyePos += transform.position;
			}
			_eyePos /= (float)_eyes.Length;
			_eyePos -= _eyes[0].up * 0.3f;
			_targetEyeRot = _defaultEyeRot;
		}
		if ((bool)questInteractable)
		{
			questInteractable.LinkNpc(this);
		}
		_serverProgressions = new byte[_quests.Count];
	}

	private void Start()
	{
		SetSkinColor();
	}

	private void SetSkinColor()
	{
		_shaderSkinColor = _skinColor;
		if ((bool)_head)
		{
			ShaderManager.SetPlayerColors(_head, _skinColor);
		}
		if ((bool)_outfit)
		{
			ShaderManager.SetPlayerColors(_outfit, _skinColor);
		}
		if ((bool)_handLeft)
		{
			ShaderManager.SetPlayerColors(_handLeft, _skinColor);
		}
		if ((bool)_handRight)
		{
			ShaderManager.SetPlayerColors(_handRight, _skinColor);
		}
	}

	private void OnDestroy()
	{
		NPCManager.RemoveNpc(this);
		_characterInteractable.OnInteract -= OnInteract;
	}

	private void EatItem(NPCQuest quest, byte questIndex, Item item)
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized && (bool)quest && (bool)item && !item.IsDestroying && !item.IsDeinitializing && !item.DeadPlayer)
		{
			if (_quests[questIndex].Type == QuestType.Money)
			{
				MoneyManager.SellItem(item);
			}
			NPCManager.Instance.SendEatEffects(_id);
			item.DestroyItem(2, _id);
			OnQuestProgression(questIndex);
		}
	}

	private void OnQuestProgression(byte questIndex)
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized && !NPCManager.Instance.NpcIsHoldingItem(_id))
		{
			_serverProgressions[questIndex]++;
			QuestLineType lineType = ((_serverProgressions[questIndex] < _quests[questIndex].TotalItems || _quests[questIndex].Type == QuestType.Money) ? QuestLineType.ItemReceived : QuestLineType.OnCompleted);
			NPCManager.Instance.ServerSpeak(_id, questIndex, _serverProgressions[questIndex], lineType);
			if (_quests[questIndex].Type != QuestType.Money && _serverProgressions[questIndex] >= _quests[questIndex].TotalItems)
			{
				NPCManager.Instance.SetShowingNpcQuest(_id, questIndex);
				_serverProgressions[questIndex] = 0;
			}
		}
	}

	private void OnInteract(Player player)
	{
		if ((bool)Server.Instance)
		{
			OnTalkedWithNPC?.Invoke();
			if (!Server.Instance.IsServerInitialized)
			{
				Server.Instance.ClientSpokeToNpc(_id);
			}
			else
			{
				NPCManager.Instance.ServerOnClientSpokeToNpc(_id);
			}
		}
	}

	public byte GetQuestIndex()
	{
		if (_quests[_curQuestIndex] == null || _curLineIndex >= _quests[_curQuestIndex].Lines.Length)
		{
			_curQuestIndex++;
			_curLineIndex = 0;
		}
		if (_curQuestIndex >= _quests.Count)
		{
			_curQuestIndex = 0;
			_curLineIndex = 0;
		}
		return _curQuestIndex;
	}

	public byte GetLineIndex(int max)
	{
		if (_curLineIndex >= max)
		{
			_curLineIndex = 0;
		}
		byte curLineIndex = _curLineIndex;
		_curLineIndex++;
		return curLineIndex;
	}

	public byte GetServerProgression(byte questIndex)
	{
		return _serverProgressions[questIndex];
	}

	private void Update()
	{
		if (_forScreenshot)
		{
			return;
		}
		MouthState mouthState = _mouthState;
		if (mouthState != MouthState.Eating && mouthState != MouthState.Speaking)
		{
			if ((bool)_targetLookingAt && !_targetLookingAt.gameObject.activeInHierarchy)
			{
				_targetLookingAt = null;
			}
			if (_mouthState == MouthState.Gaping && !_targetLookingAt)
			{
				ToggleGape(to: false);
			}
			else if (_mouthState != MouthState.Gaping && (bool)_targetLookingAt)
			{
				ToggleGape(to: true);
			}
			_targetEyeRot = GetEyeDir();
			SetEyeDir();
		}
	}

	private Quaternion GetEyeDir()
	{
		if (_eyes.Length == 0)
		{
			return Quaternion.identity;
		}
		Vector3 vector = _eyePos + _defaultEyeForward;
		if ((bool)_targetLookingAt)
		{
			vector = _targetLookingAt.position;
		}
		else
		{
			Player nearestAlivePlayer = PlayerManager.GetNearestAlivePlayer(_eyePos);
			if ((bool)nearestAlivePlayer && (nearestAlivePlayer.CamObject.position - _eyePos).sqrMagnitude < _playerEyeContactMaxDistSqr)
			{
				vector = nearestAlivePlayer.CamObject.position;
			}
		}
		Quaternion quaternion = Quaternion.LookRotation(vector - _eyePos, _eyes[0].parent.up);
		Quaternion quaternion2 = Quaternion.Inverse(_eyes[0].parent.rotation) * quaternion * Quaternion.Euler(90f, 0f, 0f);
		float num = Quaternion.Angle(_defaultEyeRot, quaternion2);
		if (num > _maxEyeRot)
		{
			quaternion2 = Quaternion.Slerp(_defaultEyeRot, quaternion2, _maxEyeRot / num);
		}
		return quaternion2;
	}

	private void SetEyeDir()
	{
		Transform[] eyes = _eyes;
		foreach (Transform obj in eyes)
		{
			obj.localRotation = Quaternion.Slerp(obj.localRotation, _targetEyeRot, _eyeRotSpeed * Time.deltaTime);
		}
	}

	public void SetNpcText(byte questIndex, byte lineIndex, byte progression, QuestLineType lineType)
	{
		_npcInteractable.OnNPCTalked();
		if (lineType == QuestLineType.OnCompleted)
		{
			QuestCompleteEffects();
		}
		if (Vector3.Distance(base.transform.position, Player.LocalPlayer.Transform.position) >= _speakingDistance)
		{
			return;
		}
		if (_quests.Count <= questIndex)
		{
			PlayerUI.SetNpcText("Quest index out of range : (", _characterInteractable.TextTarget);
			return;
		}
		NPCQuest nPCQuest = _quests[questIndex];
		if (!nPCQuest)
		{
			PlayerUI.SetNpcText($"No quest at index {questIndex} : (", _characterInteractable.TextTarget);
			return;
		}
		switch (lineType)
		{
		case QuestLineType.Default:
			ValidateNpcText(nPCQuest, nPCQuest.Lines, lineIndex, progression);
			break;
		case QuestLineType.ItemReceived:
			ValidateNpcText(nPCQuest, nPCQuest.OnItemReceivedLines, lineIndex, progression);
			break;
		case QuestLineType.OnCompleted:
			ValidateNpcText(nPCQuest, nPCQuest.OnQuestCompletedLines, lineIndex, progression);
			break;
		case QuestLineType.HoldingItem:
			ValidateNpcText(nPCQuest, nPCQuest.HoldingItemLines, lineIndex, progression);
			break;
		case QuestLineType.AlreadyCompleted:
			ValidateNpcText(nPCQuest, nPCQuest.AlreadyCompletedLines, lineIndex, progression);
			break;
		}
	}

	private void ValidateNpcText(NPCQuest quest, LocalizedString[] targetLines, byte lineIndex, byte progression)
	{
		if (lineIndex >= targetLines.Length)
		{
			PlayerUI.SetNpcText("index >= line.length : ( let dev know pls", _characterInteractable.TextTarget);
			MonoBehaviour.print($"index >= line.length: {quest.name}, lineIndex: {lineIndex}");
			return;
		}
		if (targetLines[lineIndex].GetLocalizedString() == "")
		{
			PlayerUI.SetNpcText("line at index was empty : ( let dev know pls", _characterInteractable.TextTarget);
			MonoBehaviour.print($"line at index was empty: {quest.name}, lineIndex: {lineIndex}");
			return;
		}
		string text = targetLines[lineIndex].GetLocalizedString();
		if (quest.TotalItems > 1)
		{
			text += $"\n{progression}/{quest.TotalItems}";
		}
		PlayerUI.SetNpcText(text, _characterInteractable.TextTarget);
		_speakMovementsLeft = (byte)(_speakMovements * 2);
		if (_mouthState == MouthState.None)
		{
			StartCoroutine(SpeakEffects());
		}
	}

	private IEnumerator SpeakEffects()
	{
		_mouthState = MouthState.Speaking;
		bool openMouth = true;
		if (!_mouthSource.isPlaying)
		{
			_mouthSource.time = Time.time % _mouthSource.clip.length;
			_mouthSource.Play();
		}
		LeanTween.cancel(_mouthSource.gameObject);
		LeanTween.value(_mouthSource.gameObject, _mouthSource.volume, _mouthVol, 0.1f).setOnUpdate(UpdateMouthVol);
		while (_speakMovementsLeft > 0)
		{
			if (_mouthState != MouthState.Speaking)
			{
				LeanTween.cancel(_mouthSource.gameObject);
				LeanTween.value(_mouthSource.gameObject, _mouthSource.volume, 0f, 0.1f).setOnUpdate(UpdateMouthVol).setOnComplete(MuteMouth);
				yield break;
			}
			float num = UnityEngine.Random.Range(_gapingMouthRotMinMax.x, _gapingMouthRotMinMax.y);
			float minInclusive = Mathf.InverseLerp(_gapingMouthRotMinMax.x, _gapingMouthRotMinMax.y, num);
			Vector3 to = new Vector3(UnityEngine.Random.Range(minInclusive, 1f), 1f, 1f);
			float to2 = (openMouth ? (_mouthStartRot + num) : _mouthStartRot);
			LeanTween.cancel(_mouthLower.gameObject);
			LeanTween.cancel(_mouthUpper.gameObject);
			LeanTween.value(_mouthLower.gameObject, _curMouthX, to2, _singleChewTime).setEase(openMouth ? LeanTweenType.easeOutQuad : LeanTweenType.easeInQuad).setOnUpdate(SetMouthRot);
			LeanTween.scale(_mouthLower.gameObject, to, _singleChewTime);
			LeanTween.scale(_mouthUpper.gameObject, to, _singleChewTime);
			yield return new WaitForSeconds(_singleChewTime);
			openMouth = !openMouth;
			_speakMovementsLeft--;
		}
		LeanTween.cancel(_mouthSource.gameObject);
		LeanTween.value(_mouthSource.gameObject, _mouthSource.volume, 0f, 0.1f).setOnUpdate(UpdateMouthVol).setOnComplete(MuteMouth);
		_mouthState = MouthState.None;
		LeanTween.cancel(_mouthLower.gameObject);
		LeanTween.cancel(_mouthUpper.gameObject);
		LeanTween.value(_mouthLower.gameObject, _curMouthX, _mouthStartRot, _singleChewTime).setEase(LeanTweenType.easeInQuad).setOnUpdate(SetMouthRot);
		LeanTween.scale(_mouthLower.gameObject, Vector3.one, _singleChewTime);
		LeanTween.scale(_mouthUpper.gameObject, Vector3.one, _singleChewTime);
	}

	private void UpdateMouthVol(float to)
	{
		_mouthSource.volume = to;
	}

	private void MuteMouth()
	{
		_mouthSource.Stop();
	}

	public void OnShowingQuestIndexChange(byte next)
	{
		if ((bool)questInteractable)
		{
			questInteractable.gameObject.SetActive(next != byte.MaxValue);
			MoveHand(next != byte.MaxValue);
			if (next != byte.MaxValue)
			{
				ShowQuestItem(_quests[next]);
			}
		}
	}

	public void StartEatEffects()
	{
		StartCoroutine(EatEffects());
	}

	private IEnumerator EatEffects()
	{
		if (_mouthState == MouthState.Eating)
		{
			yield break;
		}
		_mouthState = MouthState.Eating;
		bool openMouth = true;
		AudioManager.PlayClipAt("Swallow", base.transform.position, variation: true, AudioDistance.Short);
		for (int i = 0; i < _chews * 2; i++)
		{
			float to = (openMouth ? (_mouthStartRot + _gapingMouthRotMinMax.y) : _mouthStartRot);
			LeanTween.cancel(_mouthLower.gameObject);
			LeanTween.value(_mouthLower.gameObject, _curMouthX, to, _singleChewTime).setEase(openMouth ? LeanTweenType.easeOutQuad : LeanTweenType.easeInQuad).setOnUpdate(SetMouthRot);
			LeanTween.cancel(_mouthUpper.gameObject);
			LeanTween.scale(_mouthLower.gameObject, Vector3.one, _gapingMouthSpeed).setEase(LeanTweenType.easeOutQuad);
			LeanTween.scale(_mouthUpper.gameObject, Vector3.one, _gapingMouthSpeed).setEase(LeanTweenType.easeOutQuad);
			yield return new WaitForSeconds(_singleChewTime);
			if (!openMouth)
			{
				ParticleManager.Play("Spit", _mouthLower.transform.position, base.transform.forward);
			}
			openMouth = !openMouth;
		}
		_mouthState = MouthState.None;
	}

	private void ToggleGape(bool to)
	{
		if ((_mouthState != MouthState.None || to) && !((_mouthState == MouthState.Gaping) & to))
		{
			_mouthState = (to ? MouthState.Gaping : MouthState.None);
			float to2 = (to ? (_mouthStartRot + _gapingMouthRotMinMax.y) : _mouthStartRot);
			LeanTween.cancel(_mouthLower.gameObject);
			LeanTween.cancel(_mouthUpper.gameObject);
			LeanTween.value(_mouthLower.gameObject, _curMouthX, to2, _gapingMouthSpeed).setEase(LeanTweenType.easeOutBack).setOnUpdate(SetMouthRot);
			LeanTween.scale(_mouthLower.gameObject, Vector3.one, _gapingMouthSpeed).setEase(LeanTweenType.easeOutQuad);
			LeanTween.scale(_mouthUpper.gameObject, Vector3.one, _gapingMouthSpeed).setEase(LeanTweenType.easeOutQuad);
		}
	}

	private void SetMouthRot(float to)
	{
		_curMouthX = to;
		_mouthLower.localRotation = Quaternion.Euler(_curMouthX, 0f, 0f);
	}

	private void MoveHand(bool toItemPos)
	{
		LeanTween.cancel(_handHoldingItem.gameObject);
		Vector3 to = (toItemPos ? _holdingItemPos : _handStartPos);
		Vector3 to2 = (toItemPos ? _holdingItemRot : _handStartEuler);
		LeanTween.moveLocal(_handHoldingItem.gameObject, to, 0.25f).setEase(LeanTweenType.easeInOutQuad);
		LeanTween.rotateLocal(_handHoldingItem.gameObject, to2, 0.25f).setEase(LeanTweenType.easeInOutQuad);
		if (toItemPos)
		{
			_targetLookingAt = null;
			return;
		}
		LeanTween.cancel(questInteractable.gameObject);
		questInteractable.transform.localScale = Vector3.zero;
		LeanTween.scale(questInteractable.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutQuad);
	}

	private void ShowQuestItem(NPCQuest quest)
	{
		if ((bool)quest)
		{
			questInteractable.SetQuest(quest);
		}
	}

	private void QuestCompleteEffects()
	{
	}

	private bool HasGrillQuest()
	{
		foreach (NPCQuest quest in _quests)
		{
			if (quest.Type == QuestType.UnlockGrill)
			{
				return true;
			}
		}
		return false;
	}

	private void OnTriggerStay(Collider other)
	{
		if (NPCManager.Instance.NpcIsHoldingItem(_id) || _mouthState == MouthState.Eating || _quests[0].Type == QuestType.Nothing || (HasGrillQuest() && NPCManager.Instance.GrillUnlocked))
		{
			return;
		}
		Item item = ItemManager.Get(other);
		if (!item || !item.IsInteractable || item.IsDestroying || item.IsDeinitializing || !item.LastHolder || item.LastHolder.Dying.IsDead || ((bool)item.Creature && !item.Creature.IsDead && (bool)_targetLookingAt && item != _targetLookingAt))
		{
			return;
		}
		if (item == _targetLookingAt)
		{
			if ((bool)item.Holder)
			{
				return;
			}
			for (byte b = 0; b < _quests.Count; b++)
			{
				if (CanEat(_quests[b], item))
				{
					EatItem(_quests[b], b, item);
					break;
				}
			}
			return;
		}
		for (byte b2 = 0; b2 < _quests.Count; b2++)
		{
			if (CanEat(_quests[b2], item))
			{
				if ((bool)item.Holder)
				{
					_targetLookingAt = (item.Tool ? item.Tool.SwayTransform : item.transform);
				}
				else
				{
					EatItem(_quests[b2], b2, item);
				}
				break;
			}
		}
	}

	private bool CanEat(NPCQuest quest, Item item)
	{
		if ((bool)item.Creature && !item.Creature.IsDead)
		{
			return false;
		}
		if (quest.QuestItems.Count == 0)
		{
			if (!quest.OnlyCreatures || (quest.OnlyCreatures && (bool)item.Creature))
			{
				return !item.IgnoredByMoneyNpc;
			}
			return false;
		}
		if (quest.HasItem(item))
		{
			return true;
		}
		if (item.ID == GameInfo.CheatQuestItem.ID && ClientSettings.CheatsEnabled)
		{
			return true;
		}
		return false;
	}

	private void OnTriggerExit(Collider other)
	{
		if (!NPCManager.Instance.NpcIsHoldingItem(_id))
		{
			Item item = ItemManager.Get(other);
			if ((bool)item && item.IsInteractable && !item.IsDestroying && !item.IsDeinitializing && !((item.Tool ? item.Tool.SwayTransform : item.transform) != _targetLookingAt))
			{
				_targetLookingAt = null;
			}
		}
	}
}
