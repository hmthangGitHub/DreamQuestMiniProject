using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// class to handle load and save game data 
/// </summary>
public static class GameDataLoader
{
    private static string GameDataFilePath => $"{Application.persistentDataPath}/GameData.json";
    
    /// <summary>
    /// Load game data
    /// </summary>
    /// <returns></returns>
    public static GameData Load()
    {
        
        string savedGameData = default;
        
        // get game data from the file, if not exist saved data, just pass null
        if (File.Exists(GameDataFilePath))
        {
            savedGameData = File.ReadAllText(GameDataFilePath);
        }

        return GameData.Deserialize(savedGameData);
    }

    public static void Save(GameData gameData)
    {
        File.WriteAllText(GameDataFilePath, gameData.Serialize());
    }
}