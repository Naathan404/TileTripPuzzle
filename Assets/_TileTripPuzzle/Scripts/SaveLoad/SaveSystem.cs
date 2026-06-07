using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string _savePath = Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(SaveData data)
    {
        Debug.Log("[SAVE SYSTEM] Save game data");
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(_savePath, json);
    }

    public static SaveData Load()
    {
        if(!File.Exists(_savePath))
        {
            Debug.Log("[SAVE SYSTEM] No save file found -> Create new save data");
            return new SaveData();
        }
        Debug.Log("[SAVE SYSTEM] Load game data");
        string json = File.ReadAllText(_savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data;
    }

    public static void Delete()
    {
        if(File.Exists(_savePath))
        {
            File.Delete(_savePath);
            Debug.Log("[SAVE SYSTEM] Delete game data");
            return;
        }
        Debug.Log("[SAVE SYSTEM] No save file found -> Delete failed");
    }
}
