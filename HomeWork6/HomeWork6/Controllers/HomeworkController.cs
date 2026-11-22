using System;
using System.IO;
using System.IO.Compression;
using HomeWork6.Services;
using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Configuration;

namespace HomeWork6.Controllers;

[Controller(route: "homework")]
public class HomeworkController
{
    [Get(route: "send")]
    public string Send(string to)
    {
        try
        {
            var emailSettings = ConfigurationManager.Instance.EmailSettings;
            var emailService = new EmailService(emailSettings);

            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            string zipPath = Path.Combine(Path.GetTempPath(), "HomeWork6_Project.zip");

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            Console.WriteLine($"[Homework] Архивируем проект из: {projectRoot}");
            
            CreateProjectZip(projectRoot, zipPath); 
            
            Console.WriteLine($"[Homework] Архив создан: {zipPath}");

            string subject = "Домашняя работа - Айзат Сабиров";
            string body = "Группа: 507\nФИО: Сабиров Айзат\n\nВо вложении архив с исходным кодом проекта (без bin/obj).";

            emailService.SendEmail(to, subject, body, zipPath);

            Console.WriteLine("[Homework] Письмо успешно отправлено!");
            return $"Домашка отправлена на {to}. Проверь почту.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Homework] Ошибка отправки: {ex.Message}");
            return $"Ошибка: {ex.Message}";
        }
    }

    private static void CreateProjectZip(string sourceDir, string zipPath)
    {
        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        string fullSource = Path.GetFullPath(sourceDir);

        foreach (var file in Directory.GetFiles(fullSource, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(fullSource, file);
            
            if (relative.StartsWith("bin" + Path.DirectorySeparatorChar) ||
                relative.StartsWith("obj" + Path.DirectorySeparatorChar))
            {
                continue;
            }

            archive.CreateEntryFromFile(file, relative);
        }
    }
}