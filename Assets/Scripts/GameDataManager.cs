using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameDataManager : MonoBehaviour {
//  Application.persistentDataPath points to %userprofile%\AppData\Local\Packages\<productname>\LocalState.
   public static void SaveFile(GameData gameData, string sceneName) {
      string path = $"{Application.persistentDataPath}/{sceneName}.dat";
      FileStream file;
      if (File.Exists(path)) file = File.OpenWrite(path);
      else file = File.Create(path);
      BinaryFormatter bf = new BinaryFormatter();
      bf.Serialize(file, gameData);
      file.Close();
   }

   public static GameData LoadFile(string sceneName) {
      FileStream file;
      string path = $"{Application.persistentDataPath}/{sceneName}.dat";
      if(File.Exists(path)) file = File.OpenRead(path);
      else {
         Debug.LogError("File not found");
         return null;
      }
      BinaryFormatter bf = new BinaryFormatter();
      GameData data = (GameData) bf.Deserialize(file);
      file.Close();
      return data;
   }

   public static void EraseFile(string sceneName) {
      string path = $"{Application.persistentDataPath}/{sceneName}.dat";
      if(File.Exists(path)) File.Delete(path);
      Debug.Log("File Erased");
   }
}
