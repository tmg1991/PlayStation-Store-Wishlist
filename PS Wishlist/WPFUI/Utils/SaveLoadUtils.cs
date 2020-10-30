using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;


namespace WPFUI
{
   public class SaveLoadUtils
    {
       
        public static void SaveToJson(List<GameItem> gameItems, string fileName)
        {
            string json = JsonConvert.SerializeObject(gameItems, Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine(json);
            }

        }

        public static List<GameItem> LoadFromJson(string fileName)
        {
            string json;
            using (StreamReader sr = new StreamReader(fileName))
            {
                json = sr.ReadToEnd();
            }
            List<GameItem> loadedData = JsonConvert.DeserializeObject<List<GameItem>>(json);
            return loadedData;
        }
    }
}
