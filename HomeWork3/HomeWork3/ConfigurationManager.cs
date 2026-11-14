namespace HomeWork3;
using System.Text.Json;

public sealed class ConfigurationManager
{
    private static readonly Lazy<ConfigurationManager> _instance = 
        new(() => new ConfigurationManager());
    
    public static ConfigurationManager Instance => _instance.Value;
    
    public ServerSettings Settings { get; private set; }
    
    private ConfigurationManager()
    {
        Settings = LoadSettings();
    }
    
    private ServerSettings LoadSettings()
    {
        string json = File.ReadAllText("settings.json");
        return JsonSerializer.Deserialize<ServerSettings>(json) 
               ?? throw new InvalidOperationException("Не удалось загрузить настройки");
    }
}