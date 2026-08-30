using System.Text.Json;
using System.Text.Json.Serialization;
using Helix.Models;

namespace Helix.Services;

public static class SettingsStore
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string DirectoryPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Helix");

    public static string FilePath => Path.Combine(DirectoryPath, "settings.json");

    public static AppSettings Load() => LoadFrom(FilePath);

    public static AppSettings LoadFrom(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return new AppSettings();
            }

            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), Json) ?? new AppSettings();
        }
        catch (Exception ex)
        {
            AppLog.Warn("Could not read settings: " + ex.Message);
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings) => SaveTo(FilePath, settings);

    public static void SaveTo(string path, AppSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, Json);
            var temp = path + ".tmp";
            File.WriteAllText(temp, json);
            File.Copy(temp, path, overwrite: true);
            File.Delete(temp);
        }
        catch (Exception ex)
        {
            AppLog.Warn("Could not write settings: " + ex.Message);
        }
    }
}
