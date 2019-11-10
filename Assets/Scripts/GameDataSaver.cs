using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameDataSaver : MonoBehaviour
{
   public string discoveredTiles;
   public static void SaveFile(GameData gameData)
   {
      string path = Application.persistentDataPath + "/save.dat";
      FileStream file;
      if (File.Exists(path)) file = File.OpenWrite(path);
      else file = File.Create(path);
      BinaryFormatter bf = new BinaryFormatter();
      bf.Serialize(file, gameData);
      file.Close();
   }
   public static string LoadFile()
   {
      string path = Application.persistentDataPath + "/save.dat";
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
      return data.tilesDiscovered;

   }
}
