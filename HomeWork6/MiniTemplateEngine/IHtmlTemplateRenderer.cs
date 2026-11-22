namespace MiniTemplateEngine;

/// <summary>
/// Интерфейс HTML-шаблонизатора.
/// Определяет контракт для рендеринга HTML-шаблонов с подстановкой данных.
/// </summary>
public interface IHtmlTemplateRenderer
{
    /// <summary>
    /// Рендерит HTML-шаблон из строки, подставляя значения из модели данных.
    /// </summary>
    /// <param name="htmlTemplate">Строка шаблона с конструкциями ${}, $if, $foreach</param>
    /// <param name="dataModel">Объект модели данных</param>
    /// <returns>Готовая HTML-строка с подставленными значениями</returns>
    string RenderFromString(string htmlTemplate, object dataModel);

    /// <summary>
    /// Рендерит HTML-шаблон из файла, подставляя значения из модели данных.
    /// </summary>
    /// <param name="filePath">Путь к файлу шаблона</param>
    /// <param name="dataModel">Объект модели данных</param>
    /// <returns>Готовая HTML-строка с подставленными значениями</returns>
    string RenderFromFile(string filePath, object dataModel);

    /// <summary>
    /// Рендерит HTML-шаблон из файла и сохраняет результат в другой файл.
    /// </summary>
    /// <param name="inputFilePath">Путь к файлу шаблона</param>
    /// <param name="outputFilePath">Путь для сохранения результата</param>
    /// <param name="dataModel">Объект модели данных</param>
    /// <returns>Готовая HTML-строка</returns>
    string RenderToFile(string inputFilePath, string outputFilePath, object dataModel);
}