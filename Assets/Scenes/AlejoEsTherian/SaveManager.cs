using System.IO;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public string playerName;
    public int level;
    public float playTime;
    // Añade aquí los datos que quieras guardar
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
    }

    public void SaveGame(int slot, GameData data)
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(GetSavePath(slot), json);
        Debug.Log($"Partida guardada en slot {slot}");
    }

    public GameData LoadGame(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameData>(json);
        }
        return null; // El slot está vacío
    }

    public bool SlotExists(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }
}