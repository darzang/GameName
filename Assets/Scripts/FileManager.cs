using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class FileManager : MonoBehaviour {
//  Application.persistentDataPath points to %userprofile%\AppData\Local\Packages\<productname>\LocalState.
    public static void SaveLevelDataFile(LevelData levelData, string fileName) {
        string path = $"{Application.persistentDataPath}/{fileName}.dat";
        Debug.Log($"Saving levelData file at: {path}");
        FileStream file = File.Exists(path) ? File.OpenWrite(path) : File.Create(path);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, levelData);
        file.Close();
    }

    public static void SavePlayerDataFile(PlayerData playerData) {
        string path = $"{Application.persistentDataPath}/PlayerData.dat";
        Debug.Log($"Saving playerData file at: {path}");
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
            Debug.Log($"File {fileName}.dat opened");
        }
        else {
            Debug.LogError($"File {fileName}.dat not found");
            return null;
        }

        BinaryFormatter bf = new BinaryFormatter();
        LevelData data = (LevelData) bf.Deserialize(file);
        file.Close();
        Debug.Log($"File {fileName}.dat closed");
        return data;
    }
    
    public static PlayerData LoadPlayerDataFile() {
        FileStream file;
        string path = $"{Application.persistentDataPath}/PlayerData.dat";
        if (File.Exists(path)) {
            Debug.Log($"Opening playerData file at: {path}");
            file = File.OpenRead(path);
            Debug.Log($"File PlayerData.dat opened");
        }
        else {
            Debug.LogError($"File PlayerData.dat not found");
            return null;
        }

        BinaryFormatter bf = new BinaryFormatter();
        PlayerData data = (PlayerData) bf.Deserialize(file);
        file.Close();
        Debug.Log($"File PlayerData.dat closed");
        return data;
    }

    public static void DeleteFile(string fileName) {
        string path = $"{Application.persistentDataPath}/{fileName}.dat";
        if (File.Exists(path)) {
            Debug.Log($"Deleting file {fileName}.dat");
            File.Delete(path);
        }
    }
}