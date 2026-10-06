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

	public static void Init()
	{
		if (!Directory.Exists(_saveFolder))
		{
			Directory.CreateDirectory(_saveFolder);
		}
	}

	public static void SaveServer(string name, string saveString)
	{
		File.WriteAllText(_saveFolder + name + _extension, saveString);
	}

	public static void DeleteServer(string name)
	{
		string path = _saveFolder + name + _extension;
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public static List<string> LoadAllServers()
	{
		FileInfo[] files = new DirectoryInfo(_saveFolder).GetFiles("*" + _extension);
		List<string> list = new List<string>();
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
		try
		{
			File.WriteAllText(_localTempPath, saveString);
			if (!IsValidLocalSave(File.ReadAllText(_localTempPath), out error))
			{
				throw new InvalidDataException("Temporary local save failed validation: " + error);
			}
			if (File.Exists(_fullLocalSavePath))
			{
				string destinationBackupFileName = (IsValidLocalSave(File.ReadAllText(_fullLocalSavePath), out var _) ? _localBackupPath : null);
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
		if (TryReadValidLocalSave(_fullLocalSavePath, out var saveString))
		{
			return saveString;
		}
		if (!File.Exists(_localBackupPath))
		{
			return null;
		}
		Debug.LogWarning("local.txt could not be loaded. Attempting to recover the local save backup.");
		if (TryReadValidLocalSave(_localBackupPath, out saveString))
		{
			SaveLocal(saveString);
			return saveString;
		}
		Debug.LogError("Neither local.txt nor its backup contains a valid local save. Using defaults.");
		return null;
	}

	private static bool TryReadValidLocalSave(string path, out string saveString)
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
