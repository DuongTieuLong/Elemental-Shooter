using System.IO;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int highScore;
    public float bestSurvivalTime;
}

public static class SaveSystem
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "gamedata.json");

    public static void Save(GameData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Game saved successfully to: {SavePath}\nData: {json}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game data: {e.Message}");
        }
    }

    public static GameData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file found. Creating new game data.");
            return new GameData(); // Trả về dữ liệu trống mặc định
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log($"Game data loaded successfully. HighScore: {data.highScore}");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game data: {e.Message}");
            return new GameData(); // Fallback về mặc định nếu file lỗi
        }
    }
}
