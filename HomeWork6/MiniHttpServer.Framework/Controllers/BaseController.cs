using System.Net;
using MiniHttpServer.Framework.Results;
using MiniTemplateEngine;

namespace MiniHttpServer.Framework.Controllers;

/// <summary>
/// Базовый класс для всех контроллеров.
/// Предоставляет доступ к контексту HTTP-запроса и методы-помощники
/// для создания результатов (Html, Json, Page).
/// </summary>
public abstract class BaseController
{
    // Статический шаблонизатор — создаётся один раз для всех контроллеров.
    // Не нужно создавать новый экземпляр при каждом вызове Page().
    private static readonly IHtmlTemplateRenderer Renderer = new HtmlTemplateRenderer();

    // Контекст HTTP-запроса. Заполняется через SetContext() перед вызовом метода контроллера.
    protected HttpListenerContext Context { get; private set; } = null!;

    /// <summary>
    /// Устанавливает контекст HTTP-запроса для текущего контроллера.
    /// Вызывается из ControllersHandler перед выполнением метода контроллера.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса</param>
    internal void SetContext(HttpListenerContext context)
    {
        // Если контекст null, выбрасываем исключение
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Сохраняем контекст в свойство
        Context = context;
    }

    /// <summary>
    /// Создаёт HtmlResult с готовой HTML-строкой.
    /// </summary>
    /// <param name="html">Готовая HTML-строка</param>
    /// <param name="statusCode">Код HTTP-ответа (по умолчанию 200 OK)</param>
    /// <returns>Экземпляр HtmlResult</returns>
    protected IHttpResult Html(string html, int statusCode = (int)HttpStatusCode.OK)
    {
        // Создаём и возвращаем HtmlResult с переданными параметрами
        return new HtmlResult(html, statusCode);
    }

    /// <summary>
    /// Создаёт JsonResult с объектом данных, который будет сериализован в JSON.
    /// </summary>
    /// <param name="data">Объект данных для сериализации</param>
    /// <param name="statusCode">Код HTTP-ответа (по умолчанию 200 OK)</param>
    /// <returns>Экземпляр JsonResult</returns>
    protected IHttpResult Json(object data, int statusCode = (int)HttpStatusCode.OK)
    {
        // Создаём и возвращаем JsonResult с переданными параметрами
        return new JsonResult(data, statusCode);
    }

    /// <summary>
    /// Создаёт PageResult для рендеринга HTML-шаблона с моделью данных.
    /// Использует статический шаблонизатор Renderer, который создан один раз.
    /// </summary>
    /// <param name="templatePath">Путь к файлу HTML-шаблона</param>
    /// <param name="dataModel">Объект модели данных для подстановки в шаблон</param>
    /// <param name="statusCode">Код HTTP-ответа (по умолчанию 200 OK)</param>
    /// <returns>Экземпляр PageResult</returns>
    protected IHttpResult Page(string templatePath, object dataModel, int statusCode = (int)HttpStatusCode.OK)
    {
        // Создаём и возвращаем PageResult с путём к шаблону, моделью, шаблонизатором и кодом ответа
        return new PageResult(templatePath, dataModel, Renderer, statusCode);
    }
}