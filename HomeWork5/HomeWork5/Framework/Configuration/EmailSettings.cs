namespace HomeWork5.Configuration;

public class EmailSettings
{
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string SenderName { get; set; } = "Test Server";
    public string SenderEmail { get; set; } = "";
    public string Password { get; set; } = "";
}