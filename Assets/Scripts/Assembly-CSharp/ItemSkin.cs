using System;
using UnityEngine;

[Serializable]
public struct ItemSkin : IEquatable<ItemSkin>
{
	[Header("Default Settings")]
	[SerializeField]
	private string _name;

	[SerializeField]
	private Rarity _rarity;

	[Header("Coloring")]
	[SerializeField]
	private bool _useSkin;

	[SerializeField]
	private bool _isRainbowSkin;

	[SerializeField]
	private Vector2 _colorOffset;

	[SerializeField]
	private Vector2 _colorOffset2;

	[SerializeField]
	private SkinAffects _skinAffects;

	[Header("Noise Settings")]
	[SerializeField]
	private SkinNoise _noiseType;

	[SerializeField]
	private Vector2 _skinSmoothStep;

	[SerializeField]
	private float _skinNoiseScale;

	[SerializeField]
	private float _noiseRotation;

	[Header("UV Settings")]
	[SerializeField]
	private float _uvRotation;

	[SerializeField]
	private Vector2 _skinUVScale;

	[Header("Material Settings (0 = Use UV)")]
	[SerializeField]
	private float _metallicMetallicness;

	[SerializeField]
	private float _metallicSmoothness;

	[SerializeField]
	private float _plasticMetallicness;

	[SerializeField]
	private float _plasticSmoothness;

	public string Name => _name;

	public Rarity Rarity => _rarity;

	public bool UseSkin => _useSkin;

	public bool IsRainbowSkin => _isRainbowSkin;

	public SkinNoise NoiseType => _noiseType;

	public SkinAffects SkinAffects => _skinAffects;

	public Vector2 ColorOffset => _colorOffset;

	public Vector2 ColorOffset2 => _colorOffset2;

	public Vector2 SkinSmoothStep => _skinSmoothStep;

	public float SkinNoiseScale => _skinNoiseScale;

	public float NoiseRotation => _noiseRotation;

	public float UVRotation => _uvRotation;

	public Vector2 SkinUVScale => _skinUVScale;

	public float MetallicMetallicness => _metallicMetallicness;

	public float MetallicSmoothness => _metallicSmoothness;

	public float PlasticMetallicness => _plasticMetallicness;

	public float PlasticSmoothness => _plasticSmoothness;

	public ItemSkin(string name, Rarity rarity, bool useSkin, bool isRainbowSkin, SkinNoise noiseType, SkinAffects skinAffects, Vector2 colorOffset, Vector2 colorOffset2, Vector2 skinSmoothStep, float skinNoiseScale, float noiseRotation, float uvRotation, Vector2 skinUVScale, float metallicMetallicness, float metallicSmoothness, float plasticMetallicness, float plasticSmoothness)
	{
		_name = name;
		_rarity = rarity;
		_useSkin = useSkin;
		_isRainbowSkin = isRainbowSkin;
		_noiseType = noiseType;
		_skinAffects = skinAffects;
		_colorOffset = colorOffset;
		_colorOffset2 = colorOffset2;
		_skinSmoothStep = skinSmoothStep;
		_skinNoiseScale = skinNoiseScale;
		_noiseRotation = noiseRotation;
		_uvRotation = uvRotation;
		_skinUVScale = skinUVScale;
		_metallicMetallicness = metallicMetallicness;
		_metallicSmoothness = metallicSmoothness;
		_plasticMetallicness = plasticMetallicness;
		_plasticSmoothness = plasticSmoothness;
	}

	public void ChangeSettings(string name, Rarity rarity, bool useSkin, bool isRainbowSkin, SkinNoise noiseType, SkinAffects skinAffects, Vector2 colorOffset, Vector2 colorOffset2, Vector2 skinSmoothStep, float skinNoiseScale, float noiseRotation, float uvRotation, Vector2 skinUVScale, float metallicMetallicness, float metallicSmoothness, float plasticMetallicness, float plasticSmoothness)
	{
		_name = name;
		_rarity = rarity;
		_useSkin = useSkin;
		_isRainbowSkin = isRainbowSkin;
		_noiseType = noiseType;
		_skinAffects = skinAffects;
		_colorOffset = colorOffset;
		_colorOffset2 = colorOffset2;
		_skinSmoothStep = skinSmoothStep;
		_skinNoiseScale = skinNoiseScale;
		_noiseRotation = noiseRotation;
		_uvRotation = uvRotation;
		_skinUVScale = skinUVScale;
		_metallicMetallicness = metallicMetallicness;
		_metallicSmoothness = metallicSmoothness;
		_plasticMetallicness = plasticMetallicness;
		_plasticSmoothness = plasticSmoothness;
		_plasticSmoothness = plasticSmoothness;
	}

	public bool Equals(ItemSkin other)
	{
		return _name == other._name;
	}

	public override bool Equals(object obj)
	{
		if (obj is ItemSkin other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (_name == null)
		{
			return 0;
		}
		return _name.GetHashCode();
	}
}
