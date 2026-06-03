using System.Text.Json;


namespace Infrastructure
{
    /// <summary>
    /// PlaceHolder class for saving last-date-accesed dictionary for client system guids
    /// </summary>
    public static class JsonLoader
    {
        private static readonly string _folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NetVine"
        );
        public static void SaveToFile<T>(T o, string fileName)
        {
            string json = JsonSerializer.Serialize(o);
            File.WriteAllText(Path.Combine(_folderPath, fileName), json);
        }

        public static T? LoadFromFile<T>(string fileName)
        {
            string jsonStr = File.ReadAllText(Path.Combine(_folderPath, fileName));
            return JsonSerializer.Deserialize<T>(jsonStr);
        }



        public static bool TryLoadFromFile<T>(out T? o, string fileName)
        {
            try
            {
                o = LoadFromFile<T>(fileName);
                return o != null;
            }
            catch (Exception e) when (e is JsonException or IOException or ArgumentException)
            {
                o = default;
                return false;
            }

        }

        public static bool TrySaveToFile<T>(T o, string filePath)
        {
            if (o == null) return false;

            try
            {
                SaveToFile(o, filePath);
                return true;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException)
            {
                return false;
            }
        }



    }
}
