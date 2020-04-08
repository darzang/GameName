using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class FileManager : MonoBehaviour {
//  Application.persistentDataPath points to %userprofile%\AppData\Local\Packages\<productname>\LocalState.
    public static void SaveLevelDataFile(LevelData levelData, string fileName) {
        Debug.Log($"Saving levelData file: \n {JsonUtility.ToJson(levelData, true)}");
        if (levelData == null) {
            // Yep, that's harsh
            DeleteFile(fileName);
        }
        string path = $"{Application.persistentDataPath}/{fileName}.dat";
        FileStream file = File.Exists(path) ? File.OpenWrite(path) : File.Create(path);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, levelData);
        file.Close();
    }

    public static void SavePlayerDataFile(PlayerData playerData) {
        Debug.Log($"Saving playerData file: \n {JsonUtility.ToJson(playerData, true)}");

        string path = $"{Application.persistentDataPath}/PlayerData.dat";
        FileStream file = File.Exists(path) ? File.OpenWrite(path) : File.Create(path);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, playerData);
        file.Close();
    }

    public static LevelData LoadLevelDataFile(string fileName) {
        FileStream file;
        string path = $"{Application.persistentDataPath}/{fileName}.dat";
        if (File.Exists(path)) {
            Debug.Log($"Opening levelData file at: {path}");
            file = File.OpenRead(path);
        }
        else {
            Debug.LogWarning($"File {fileName}.dat not found");
            return null;
        }

        BinaryFormatter bf = new BinaryFormatter();
        LevelData data = (LevelData) bf.Deserialize(file);
        file.Close();
        // Debug.Log($"levelData loaded: \n {JsonUtility.ToJson(data, true)}");
        if (data.mazeCellsForFile.Find(cell => cell.permanentlyRevealed) != null) {
            Debug.Log("levelData has permanently discovered cells");
        }
        return data;
    }
    
    public static PlayerData LoadPlayerDataFile() {
        FileStream file;
        string path = $"{Application.persistentDataPath}/PlayerData.dat";
        if (File.Exists(path)) {
            Debug.Log($"Opening playerData file at: {path}");
            file = File.OpenRead(path);
        }
        else {
            Debug.LogWarning("File PlayerData.dat not found");
            return null;
        }

        BinaryFormatter bf = new BinaryFormatter();
        PlayerData data = (PlayerData) bf.Deserialize(file);
        file.Close();
        Debug.Log($"PlayerData loaded: \n {JsonUtility.ToJson(data, true)}");
        return data;
    }

    public static void DeleteFile(string fileName) {
        string path = $"{Application.persistentDataPath}/{fileName}.dat";
        if (!File.Exists(path)) return;
        Debug.Log($"Deleting file {fileName}.dat");
        File.Delete(path);
    }
}