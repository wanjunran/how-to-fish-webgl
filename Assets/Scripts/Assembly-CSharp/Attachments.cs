using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.Serialization;

public class Attachments : NetworkBehaviour
{
	[SerializeField]
	private List<Sight> _sights;

	[SerializeField]
	private List<BarrelAttachment> _barrelAttachments;

	[FormerlySerializedAs("_bulletUpgradeDamages")]
	[SerializeField]
	private BulletUpgrade[] _bulletUpgrades;

	[Space]
	[SerializeField]
	private int _defaultAmmoPerMag;

	[SerializeField]
	private int _extendedAmmoPerMag;

	[SerializeField]
	private LaserSight _laserSight;

	[SerializeField]
	private AttachmentInfo _extendedMagInfo;

	[SerializeField]
	private int _extendedMagCost;

	public readonly SyncVar<byte> _syncedSight = new SyncVar<byte>();

	public readonly SyncVar<byte> _syncedBarrelAttachment = new SyncVar<byte>();

	public readonly SyncVar<byte> _syncedBulletIndex = new SyncVar<byte>();

	public readonly SyncVar<bool> _syncedExtendedMag = new SyncVar<bool>();

	public readonly SyncVar<bool> _syncedLaserSight = new SyncVar<bool>();

	private Dictionary<AttachmentInfo, int> _attachmentCosts = new Dictionary<AttachmentInfo, int>();

	private bool NetworkInitialize___EarlyAttachmentsAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateAttachmentsAssembly_002DCSharp_002Edll_Excuted;

	public Weapon Weapon { get; private set; }

	public byte Sight => _syncedSight.Value;

	public byte BarrelAttachment => _syncedBarrelAttachment.Value;

	public byte AmmoType => _syncedBulletIndex.Value;

	public bool ExtendedMag => _syncedExtendedMag.Value;

	public bool LaserSight => _syncedLaserSight.Value;

	public Vector3 AdsPos => _sights[_syncedSight.Value].AdsPos;

	public float AdsFov => _sights[_syncedSight.Value].AdsFov;

	public float AimSwayMulti => _sights[_syncedSight.Value].AimSwayMulti;

	public float AdsSpeedDamping => _sights[_syncedSight.Value].AdsSpeedDamping;

	public bool UseSniperUi => _sights[_syncedSight.Value].UseSniperUi;

	public float SniperUiAimPercent => _sights[_syncedSight.Value].SniperUiAimPercent;

	public Transform SniperUiPos => _sights[_syncedSight.Value].SniperUiPos;

	public AnimationCurve SniperUiPosCurve => _sights[_syncedSight.Value].SniperUiPosCurve;

	public float SniperUiScale => _sights[_syncedSight.Value].SniperUiScale;

	public Transform FirePoint => _barrelAttachments[_syncedBarrelAttachment.Value].FirePoint;

	public ParticleSystem FireParticle => _barrelAttachments[_syncedBarrelAttachment.Value].FireParticle;

	public string FireSound => _barrelAttachments[_syncedBarrelAttachment.Value].GetFireSound();

	public float FireSoundVolume => _barrelAttachments[_syncedBarrelAttachment.Value].FireSoundVolume;

	public bool UseMediumSoundDistance => _barrelAttachments[_syncedBarrelAttachment.Value].UseMediumSoundDistance;

	public int FireSoundCount => _barrelAttachments[_syncedBarrelAttachment.Value].FireSoundCount;

	public float AdsRecoilPosMulti => _barrelAttachments[_syncedBarrelAttachment.Value].AdsRecoilPosMulti;

	public float AdsRecoilRotMulti => _barrelAttachments[_syncedBarrelAttachment.Value].AdsRecoilRotMulti;

	public float AdsRecoilSpringMulti => _barrelAttachments[_syncedBarrelAttachment.Value].AdsRecoilSpringMulti;

	public Vector2 ScreenRecoilAmount => _barrelAttachments[_syncedBarrelAttachment.Value].ScreenRecoilAmount;

	public Vector3 ModelRecoilPos => _barrelAttachments[_syncedBarrelAttachment.Value].ModelRecoilPos;

	public Vector2 ModelRecoilRot => _barrelAttachments[_syncedBarrelAttachment.Value].ModelRecoilRot;

	public float WeaponRecoilMulti => _barrelAttachments[_syncedBarrelAttachment.Value].WeaponRecoilMulti;

	public float RecoilSpringPos => _barrelAttachments[_syncedBarrelAttachment.Value].RecoilSpringPos;

	public float RecoilDamperPos => _barrelAttachments[_syncedBarrelAttachment.Value].RecoilDamperPos;

	public float RecoilSpringRot => _barrelAttachments[_syncedBarrelAttachment.Value].RecoilSpringRot;

	public float RecoilDamperRot => _barrelAttachments[_syncedBarrelAttachment.Value].RecoilDamperRot;

	public int Damage => _bulletUpgrades[_syncedBulletIndex.Value].Damage;

	public int AmmoPerMag
	{
		get
		{
			if (!_syncedExtendedMag.Value)
			{
				return _defaultAmmoPerMag;
			}
			return _extendedAmmoPerMag;
		}
	}

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Attachments_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public void InitAttachments(Weapon weapon)
	{
		Weapon = weapon;
		foreach (Sight sight in _sights)
		{
			if ((bool)sight.Info)
			{
				_attachmentCosts.Add(sight.Info, sight.Cost);
			}
		}
		foreach (BarrelAttachment barrelAttachment in _barrelAttachments)
		{
			if ((bool)barrelAttachment.Info)
			{
				_attachmentCosts.Add(barrelAttachment.Info, barrelAttachment.Cost);
			}
		}
		if ((bool)_laserSight.Info)
		{
			_attachmentCosts.Add(_laserSight.Info, _laserSight.Cost);
		}
		if ((bool)_extendedMagInfo)
		{
			_attachmentCosts.Add(_extendedMagInfo, _extendedMagCost);
		}
	}

	private void DisableAttachments()
	{
		foreach (Sight sight in _sights)
		{
			sight.gameObject.SetActive(value: false);
		}
		foreach (BarrelAttachment barrelAttachment in _barrelAttachments)
		{
			barrelAttachment.gameObject.SetActive(value: false);
		}
		_laserSight.gameObject.SetActive(value: false);
	}

	public override void OnStartClient()
	{
		OnSightChange(0, _syncedSight.Value, asServer: false);
		OnBarrelChange(0, _syncedBarrelAttachment.Value, asServer: false);
		OnMagChange(prev: false, _syncedExtendedMag.Value, asServer: false);
		OnLaserSightChange(prev: false, _syncedLaserSight.Value, asServer: false);
	}

	private void OnLaserSightChange(bool prev, bool next, bool asServer)
	{
		if ((bool)_laserSight)
		{
			_laserSight.gameObject.SetActive(next);
		}
		AchievementManager.CheckAttachmentsAchievement(this);
	}

	private void OnMagChange(bool prev, bool next, bool asServer)
	{
		AchievementManager.CheckAttachmentsAchievement(this);
	}

	private void OnAmmoChange(byte prev, byte next, bool asServer)
	{
		AchievementManager.CheckAttachmentsAchievement(this);
	}

	private void OnBarrelChange(byte prev, byte next, bool asServer)
	{
		BarrelAttachment barrelAttachment = _barrelAttachments[prev];
		if ((bool)barrelAttachment)
		{
			barrelAttachment.gameObject.SetActive(value: false);
		}
		BarrelAttachment barrelAttachment2 = _barrelAttachments[next];
		if ((bool)barrelAttachment2)
		{
			barrelAttachment2.gameObject.SetActive(value: true);
		}
		Weapon.SetRecoilSprings();
		AchievementManager.CheckAttachmentsAchievement(this);
	}

	private void OnSightChange(byte prev, byte next, bool asServer)
	{
		if ((bool)Weapon.Holder && Weapon.Holder.Owner.IsLocalClient)
		{
			Weapon.SecondaryInputCanceled(default);
			PlayerUI.ToggleSniperUI(to: false);
			ToggleSightModels(to: true, prev);
		}
		Sight sight = _sights[prev];
		if ((bool)sight)
		{
			sight.gameObject.SetActive(value: false);
		}
		Sight sight2 = _sights[next];
		if ((bool)sight2)
		{
			sight2.gameObject.SetActive(value: true);
		}
		AchievementManager.CheckAttachmentsAchievement(this);
	}

	public void SetAttachment(AttachmentInfo info)
	{
		if (!CanAttach(info) || HasAttachment(info))
		{
			return;
		}
		for (int i = 0; i < _sights.Count; i++)
		{
			if (_sights[i].Info == info)
			{
				_syncedSight.Value = (byte)i;
				return;
			}
		}
		for (int j = 0; j < _barrelAttachments.Count; j++)
		{
			if (_barrelAttachments[j].Info == info)
			{
				_syncedBarrelAttachment.Value = (byte)j;
				return;
			}
		}
		if (_laserSight.Info == info)
		{
			_syncedLaserSight.Value = true;
		}
		else if (_extendedMagInfo == info)
		{
			_syncedExtendedMag.Value = true;
		}
	}

	public void UpgradeBullets()
	{
		byte value = _syncedBulletIndex.Value;
		value++;
		value = (byte)Mathf.Clamp(value, 0, _bulletUpgrades.Length);
		_syncedBulletIndex.Value = value;
	}

	public BulletUpgrade GetCurBulletUpgrade()
	{
		return _bulletUpgrades[_syncedBulletIndex.Value];
	}

	public BulletUpgrade GetNextBulletUpgrade(byte max = byte.MaxValue)
	{
		if (_syncedBulletIndex.Value + 1 >= _bulletUpgrades.Length || max <= _syncedBulletIndex.Value)
		{
			return null;
		}
		return _bulletUpgrades[_syncedBulletIndex.Value + 1];
	}

	public bool CanAttach(AttachmentInfo info)
	{
		return _attachmentCosts.ContainsKey(info);
	}

	public int GetAttachmentCost(AttachmentInfo info)
	{
		if (_attachmentCosts.ContainsKey(info))
		{
			return _attachmentCosts[info];
		}
		return 0;
	}

	public bool HasAttachment(AttachmentInfo info)
	{
		if (_sights[_syncedSight.Value].Info == info)
		{
			return true;
		}
		if (_barrelAttachments[_syncedBarrelAttachment.Value].Info == info)
		{
			return true;
		}
		if (_laserSight.Info == info && _syncedLaserSight.Value)
		{
			return true;
		}
		if (_extendedMagInfo == info && _syncedExtendedMag.Value)
		{
			return true;
		}
		return false;
	}

	public void ToggleSightModels(bool to, byte overrideWith = byte.MaxValue)
	{
		byte index = ((overrideWith == byte.MaxValue) ? _syncedSight.Value : overrideWith);
		GameObject[] disableWhenSniperUi = _sights[index].DisableWhenSniperUi;
		for (int i = 0; i < disableWhenSniperUi.Length; i++)
		{
			disableWhenSniperUi[i].SetActive(to);
		}
		_barrelAttachments[_syncedBarrelAttachment.Value].gameObject.SetActive(to);
		if (_syncedLaserSight.Value)
		{
			_laserSight.gameObject.SetActive(to);
		}
	}

	public string GetAttachmentInfo()
	{
		string text = "";
		int num = (_syncedExtendedMag.Value ? _extendedAmmoPerMag : _defaultAmmoPerMag);
		text += $"<b>{num}</b> {LocalizationManager.AmmoLocalized.GetLocalizedString()}";
		text += $"\n<b>{_bulletUpgrades[_syncedBulletIndex.Value].Damage}</b> {LocalizationManager.DamageLocalized.GetLocalizedString()}";
		text += "\n\n";
		bool flag = false;
		if (_syncedSight.Value != 0 && (bool)_sights[_syncedSight.Value].Info)
		{
			text = text + "<b>" + LocalizationManager.AttachmentsLocalized.GetLocalizedString() + "</b>\n";
			text += _sights[_syncedSight.Value].Info.NameLocalized;
			flag = true;
		}
		if (_syncedBarrelAttachment.Value != 0 && (bool)_barrelAttachments[_syncedBarrelAttachment.Value].Info)
		{
			text += (flag ? "\n" : ("<b>" + LocalizationManager.AttachmentsLocalized.GetLocalizedString() + "</b>\n"));
			text += _barrelAttachments[_syncedBarrelAttachment.Value].Info.NameLocalized;
			flag = true;
		}
		if (_syncedLaserSight.Value && (bool)_laserSight.Info)
		{
			text += (flag ? "\n" : ("<b>" + LocalizationManager.AttachmentsLocalized.GetLocalizedString() + "</b>\n"));
			text += _laserSight.Info.NameLocalized;
			flag = true;
		}
		if (_syncedExtendedMag.Value && (bool)_extendedMagInfo)
		{
			text += (flag ? "\n" : ("<b>" + LocalizationManager.AttachmentsLocalized.GetLocalizedString() + "</b>\n"));
			text += _extendedMagInfo.NameLocalized;
			flag = true;
		}
		return text;
	}

	public void LoadFromSave(SavedItem savedItem)
	{
		_syncedSight.Value = savedItem.Sight;
		_syncedBarrelAttachment.Value = savedItem.BarrelAttachment;
		_syncedBulletIndex.Value = savedItem.AmmoType;
		_syncedExtendedMag.Value = savedItem.ExtendedMag;
		_syncedLaserSight.Value = savedItem.LaserSight;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyAttachmentsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyAttachmentsAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_syncedLaserSight.InitializeEarly(this, 4u, isSyncObject: false);
			_syncedExtendedMag.InitializeEarly(this, 3u, isSyncObject: false);
			_syncedBulletIndex.InitializeEarly(this, 2u, isSyncObject: false);
			_syncedBarrelAttachment.InitializeEarly(this, 1u, isSyncObject: false);
			_syncedSight.InitializeEarly(this, 0u, isSyncObject: false);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateAttachmentsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateAttachmentsAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_syncedLaserSight.InitializeLate();
			_syncedExtendedMag.InitializeLate();
			_syncedBulletIndex.InitializeLate();
			_syncedBarrelAttachment.InitializeLate();
			_syncedSight.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void Awake_UserLogic_Attachments_Assembly_002DCSharp_002Edll()
	{
		_syncedSight.OnChange += OnSightChange;
		_syncedBarrelAttachment.OnChange += OnBarrelChange;
		_syncedBulletIndex.OnChange += OnAmmoChange;
		_syncedExtendedMag.OnChange += OnMagChange;
		_syncedLaserSight.OnChange += OnLaserSightChange;
		DisableAttachments();
	}
}
