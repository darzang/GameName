using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
   public List<string> discoveredTiles;
   static string path = Application.persistentDataPath + "/save.dat"; // points to %userprofile%\AppData\Local\Packages\<productname>\LocalState.
   public static void SaveFile(GameData gameData)
   {
      FileStream file;
      if (File.Exists(path)) file = File.OpenWrite(path);
      else file = File.Create(path);
      BinaryFormatter bf = new BinaryFormatter();
      bf.Serialize(file, gameData);
      file.Close();
   }
   public static GameData LoadFile()
   {
      FileStream file;

      if(File.Exists(path)) file = File.OpenRead(path);
      else
      {
         Debug.LogError("File not found");
         return null;
      }

      BinaryFormatter bf = new BinaryFormatter();
      GameData data = (GameData) bf.Deserialize(file);

      file.Close();
      return data;

   }

   public static void EraseFile()
   {
      if(File.Exists(path)) File.Delete(path);
      Debug.Log("File Erased");
   }
}
