using System.Text.Json;

namespace HomeWork5.Configuration;

public sealed class ConfigurationManager
{
    private static readonly Lazy<ConfigurationManager> _instance = 
        new(() => new ConfigurationManager());
    
    public static ConfigurationManager Instance => _instance.Value;
    public EmailSettings EmailSettings { get; private set; }
    
    public ServerSettings Settings { get; private set; }
    
    private ServerSettings LoadSettings()
    {
        string json = File.ReadAllText("settings.json");
        return JsonSerializer.Deserialize<ServerSettings>(json) 
               ?? throw new InvalidOperationException("Не удалось загрузить настройки");
    }
    
    private EmailSettings LoadEmailSettings()
    {
        string json = File.ReadAllText("email-settings.json");
        return JsonSerializer.Deserialize<EmailSettings>(json) 
               ?? throw new InvalidOperationException("Не удалось загрузить email-settings.json");
    }
    private ConfigurationManager()
    {
        Settings = LoadSettings();
        EmailSettings = LoadEmailSettings();
    }
}