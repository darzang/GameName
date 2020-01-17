using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameDataManager : MonoBehaviour {
//  Application.persistentDataPath points to %userprofile%\AppData\Local\Packages\<productname>\LocalState.
   public static void SaveFile(GameData gameData, string sceneName) {
      string path = $"{Application.persistentDataPath}/{sceneName}.dat";
      Debug.Log($"Saving file at: {path}");
      FileStream file = File.Exists(path) ? File.OpenWrite(path) : File.Create(path);
      BinaryFormatter bf = new BinaryFormatter();
      bf.Serialize(file, gameData);
      file.Close();
   }

   public static GameData LoadFile(string sceneName) {
      FileStream file;
      string path = $"{Application.persistentDataPath}/{sceneName}.dat";
      Debug.Log($"Opening file at: {path}");
      if (File.Exists(path)) {
         file = File.OpenRead(path);
      } else {
         Debug.LogError("File not found");
         return null;
      }
      BinaryFormatter bf = new BinaryFormatter();
      GameData data = (GameData) bf.Deserialize(file);
      file.Close();
      return data;
   }

   public static void ResetFile(string sceneName) {
      string path = $"{Application.persistentDataPath}/{sceneName}.dat";
      Debug.Log($"Resetting file at: {path}");
      if (File.Exists(path)) {
         GameData data = LoadFile(path);
         data.tryCount = 0;
         data.mapFragments = new List<Fragment>();
         data.totalDiscoveredTiles = new List<string>();
         SaveFile(data, sceneName);
         Debug.Log("File reset");
         File.Delete(path);
      }
   }
}
