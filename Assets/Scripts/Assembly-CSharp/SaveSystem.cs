// ============================================================
//  WebGL 存档后端
// ============================================================
//  问题：原SaveSystem 依赖 System.IO.File（File.WriteAllText /
//  File.Replace / DirectoryInfo 等）。WebGL 平台没有真实文件系统，
//  File API 会抛异常或静默失败，导致存档功能不可用。
//
//  方案：抽出一个存储抽象层，按平台切换后端：
//    - 桌面平台（Editor/Standalone）：继续用真实文件（行为不变）
//    - WebGL：用 PlayerPrefs（底层由浏览器 IndexedDB 持久化，
//            容量通常 1MB~5MB，足够存本游戏的存档）
//
//  保持 SaveSystem 的公开 API 签名不变，调用方（SaveManager、
//  AutoSaver 等）无需任何修改。
//
//  注意：PlayerPrefs 单键上限约 1MB / 总容量因浏览器而异。
//  若后续存档体积变大，需改接 JS 层的 indexedDB 插件。
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
	private static readonly string _saveFolder = Application.persistentDataPath + "/Saves/";

	private static readonly string _localFileName = "local";

	private static readonly string _extension = ".txt";

	private static readonly string _fullLocalSavePath = _saveFolder + _localFileName + _extension;

	private static readonly string _localBackupPath = _fullLocalSavePath + ".backup";

	private static readonly string _localTempPath = _fullLocalSavePath + ".tmp";

	/// <summary>是否运行在 WebGL 平台</summary>
	private static bool IsWebGL
	{
		get
		{
#if UNITY_WEBGL && !UNITY_EDITOR
			return true;
#else
			return false;
#endif
		}
	}

	// PlayerPrefs 键名
	private const string WebGLKeyLocal = "htf_save_local";
	private const string WebGLKeyLocalBackup = "htf_save_local_backup";
	private const string WebGLKeyServerPrefix = "htf_save_server_";
	private const string WebGLKeyServerIndex = "htf_save_server_index";

	public static void Init()
	{
		if (!IsWebGL)
		{
			if (!Directory.Exists(_saveFolder))
			{
				Directory.CreateDirectory(_saveFolder);
			}
		}
		else
		{
			Debug.Log("[WebGL] 存档后端：PlayerPrefs（浏览器 IndexedDB）。");
		}
	}

	public static void SaveServer(string name, string saveString)
	{
		if (IsWebGL)
		{
			// 用 URL 安全编码，避免键名中的特殊字符被 PlayerPrefs 拒绝
			PlayerPrefs.SetString(WebGLKeyServerPrefix + Uri.EscapeDataString(name), saveString);

			// 维护存档名索引（PlayerPrefs 无法枚举键）
			List<string> index = LoadServerIndex();
			string encoded = Uri.EscapeDataString(name);
			if (!index.Contains(encoded))
			{
				index.Add(encoded);
				SaveServerIndex(index);
			}

			PlayerPrefs.Save();
			return;
		}

		File.WriteAllText(_saveFolder + name + _extension, saveString);
	}

	public static void DeleteServer(string name)
	{
		if (IsWebGL)
		{
			PlayerPrefs.DeleteKey(WebGLKeyServerPrefix + Uri.EscapeDataString(name));

			List<string> index = LoadServerIndex();
			index.Remove(Uri.EscapeDataString(name));
			SaveServerIndex(index);

			PlayerPrefs.Save();
			return;
		}

		string path = _saveFolder + name + _extension;
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public static List<string> LoadAllServers()
	{
		List<string> list = new List<string>();

		if (IsWebGL)
		{
			foreach (string encoded in LoadServerIndex())
			{
				string value = PlayerPrefs.GetString(WebGLKeyServerPrefix + encoded, null);
				if (!string.IsNullOrWhiteSpace(value))
				{
					list.Add(value);
				}
			}
			return list;
		}

		FileInfo[] files = new DirectoryInfo(_saveFolder).GetFiles("*" + _extension);
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			if (!(fileInfo.Name == _localFileName + _extension))
			{
				string text = File.ReadAllText(fileInfo.FullName);
				if (string.IsNullOrWhiteSpace(text))
				{
					File.Delete(fileInfo.FullName);
				}
				else
				{
					list.Add(text);
				}
			}
		}
		return list;
	}

	public static void SaveLocal(string saveString)
	{
		if (!IsValidLocalSave(saveString, out var error))
		{
			Debug.LogError("Local save was not written because it is invalid: " + error);
			return;
		}

		if (IsWebGL)
		{
			// PlayerPrefs 没有原子替换语义，用"先备份再写入"模拟
			if (PlayerPrefs.HasKey(WebGLKeyLocal))
			{
				string existing = PlayerPrefs.GetString(WebGLKeyLocal, null);
				if (IsValidLocalSave(existing, out _))
				{
					PlayerPrefs.SetString(WebGLKeyLocalBackup, existing);
				}
			}

			PlayerPrefs.SetString(WebGLKeyLocal, saveString);
			PlayerPrefs.Save();

			// 回读校验
			string written = PlayerPrefs.GetString(WebGLKeyLocal, null);
			if (!IsValidLocalSave(written, out var verifyError))
			{
				Debug.LogError("Local save verification failed: " + verifyError);
			}
			return;
		}

		// ---- 桌面平台：保留原原子写入逻辑 ----
		try
		{
			File.WriteAllText(_localTempPath, saveString);
			if (!IsValidLocalSave(File.ReadAllText(_localTempPath), out error))
			{
				throw new InvalidDataException("Temporary local save failed validation: " + error);
			}
			if (File.Exists(_fullLocalSavePath))
			{
				string error2;
				string destinationBackupFileName = (IsValidLocalSave(File.ReadAllText(_fullLocalSavePath), out error2) ? _localBackupPath : null);
				File.Replace(_localTempPath, _fullLocalSavePath, destinationBackupFileName);
			}
			else
			{
				File.Move(_localTempPath, _fullLocalSavePath);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Could not safely write local save: {arg}");
		}
		finally
		{
			try
			{
				if (File.Exists(_localTempPath))
				{
					File.Delete(_localTempPath);
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Could not remove temporary local save: " + ex.Message);
			}
		}
	}

	public static string LoadLocal()
	{
		if (IsWebGL)
		{
			if (TryReadValidLocalSave(PlayerPrefs.GetString(WebGLKeyLocal, null), out var saveString))
			{
				return saveString;
			}
			Debug.LogWarning("Local save could not be loaded. Attempting to recover from backup.");
			if (TryReadValidLocalSave(PlayerPrefs.GetString(WebGLKeyLocalBackup, null), out saveString))
			{
				SaveLocal(saveString);
				return saveString;
			}
			Debug.LogError("Neither local save nor its backup contains a valid save. Using defaults.");
			return null;
		}

		if (TryReadValidLocalSaveFromFile(_fullLocalSavePath, out var desktopSave))
		{
			return desktopSave;
		}
		if (!File.Exists(_localBackupPath))
		{
			return null;
		}
		Debug.LogWarning("local.txt could not be loaded. Attempting to recover the local save backup.");
		if (TryReadValidLocalSaveFromFile(_localBackupPath, out desktopSave))
		{
			SaveLocal(desktopSave);
			return desktopSave;
		}
		Debug.LogError("Neither local.txt nor its backup contains a valid local save. Using defaults.");
		return null;
	}

	// ---- 内部工具 ----

	private static List<string> LoadServerIndex()
	{
		List<string> index = new List<string>();
		string raw = PlayerPrefs.GetString(WebGLKeyServerIndex, string.Empty);
		if (string.IsNullOrEmpty(raw))
		{
			return index;
		}
		foreach (string s in raw.Split('|'))
		{
			if (!string.IsNullOrEmpty(s))
			{
				index.Add(s);
			}
		}
		return index;
	}

	private static void SaveServerIndex(List<string> index)
	{
		PlayerPrefs.SetString(WebGLKeyServerIndex, string.Join("|", index));
	}

	/// <summary>WebGL 版：直接从字符串校验（不涉及文件）</summary>
	private static bool TryReadValidLocalSave(string content, out string saveString)
	{
		saveString = null;
		if (string.IsNullOrWhiteSpace(content))
		{
			return false;
		}
		if (IsValidLocalSave(content, out var error))
		{
			saveString = content;
			return true;
		}
		Debug.LogWarning("Local save is invalid: " + error);
		return false;
	}

	/// <summary>桌面版：从文件路径校验</summary>
	private static bool TryReadValidLocalSaveFromFile(string path, out string saveString)
	{
		saveString = null;
		if (!File.Exists(path))
		{
			return false;
		}
		try
		{
			saveString = File.ReadAllText(path);
			if (IsValidLocalSave(saveString, out var error))
			{
				return true;
			}
			Debug.LogWarning("Local save '" + path + "' is invalid: " + error);
		}
		catch (Exception arg)
		{
			Debug.LogWarning($"Could not read local save '{path}': {arg}");
		}
		saveString = null;
		return false;
	}

	private static bool IsValidLocalSave(string saveString, out string error)
	{
		if (string.IsNullOrWhiteSpace(saveString))
		{
			error = "The file is empty.";
			return false;
		}
		try
		{
			if (JsonUtility.FromJson<SaveManager.LocalSaveObject>(saveString) == null)
			{
				error = "JSON produced a null save object.";
				return false;
			}
			error = null;
			return true;
		}
		catch (Exception ex)
		{
			error = ex.Message;
			return false;
		}
	}
}