using System.Net;
using System.Text;
using MiniTemplateEngine;

namespace MiniHttpServer.Framework.Results;

/// <summary>
/// Реализация IHttpResult для формирования HTTP-ответа на основе HTML-шаблона и модели данных.
/// Использует IHtmlTemplateRenderer для рендеринга шаблона.
/// Делегирует запись в ответ классу HtmlResult, чтобы не дублировать логику работы с потоками.
/// </summary>
public class PageResult : IHttpResult
{
    // Путь к файлу HTML-шаблона
    private readonly string _templatePath;

    // Объект модели данных, значения свойств которого подставляются в шаблон
    private readonly object _dataModel;

    // Экземпляр шаблонизатора, который выполняет рендеринг
    private readonly IHtmlTemplateRenderer _renderer;

    // HTTP-код ответа (по умолчанию 200 OK)
    private readonly int _statusCode;

    /// <summary>
    /// Инициализирует новый экземпляр PageResult.
    /// </summary>
    /// <param name="templatePath">Путь к файлу HTML-шаблона</param>
    /// <param name="dataModel">Объект модели данных для подстановки в шаблон</param>
    /// <param name="renderer">Экземпляр шаблонизатора для рендеринга</param>
    /// <param name="statusCode">Код HTTP-ответа (по умолчанию 200 OK)</param>
    public PageResult(string templatePath, object dataModel, IHtmlTemplateRenderer renderer, int statusCode = 200)
    {
        // Сохраняем путь к шаблону в приватное поле
        _templatePath = templatePath;

        // Сохраняем модель данных в приватное поле
        _dataModel = dataModel;

        // Сохраняем ссылку на шаблонизатор в приватное поле
        _renderer = renderer;

        // Сохраняем код HTTP-ответа в приватное поле
        _statusCode = statusCode;
    }

    /// <summary>
    /// Выполняет рендеринг шаблона и записывает результат в HTTP-ответ.
    /// Делегирует запись классу HtmlResult для переиспользования логики.
    /// </summary>
    /// <param name="response">HTTP-ответ для записи результата</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции</param>
    public async Task ExecuteAsync(HttpListenerResponse response, CancellationToken cancellationToken = default)
    {
        // Рендерим HTML из файла шаблона, подставляя значения из модели данных
        string html = _renderer.RenderFromFile(_templatePath, _dataModel);

        // Создаём HtmlResult с готовой HTML-строкой и кодом ответа
        // Это позволяет переиспользовать логику записи из HtmlResult
        HtmlResult htmlResult = new HtmlResult(html, _statusCode);

        // Делегируем запись в HTTP-ответ экземпляру HtmlResult
        await htmlResult.ExecuteAsync(response, cancellationToken);
    }
}