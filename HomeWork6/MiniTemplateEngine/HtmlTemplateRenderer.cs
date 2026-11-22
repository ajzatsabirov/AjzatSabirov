using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniTemplateEngine;

/// <summary>
/// Реализация HTML-шаблонизатора.
/// Поддерживает подстановку переменных ${Property}, условные конструкции $if/$else/$endif
/// и циклы $foreach(var item in Collection)/$endfor.
/// Использует рефлексию для получения значений свойств модели.
/// </summary>
public class HtmlTemplateRenderer : IHtmlTemplateRenderer
{
    /// <summary>
    /// Рендерит шаблон из строки, обрабатывая все конструкции.
    /// </summary>
    /// <param name="htmlTemplate">Строка шаблона</param>
    /// <param name="dataModel">Объект модели данных</param>
    /// <returns>Готовая HTML-строка</returns>
    public string RenderFromString(string htmlTemplate, object dataModel)
    {
        // Если шаблон пустой, возвращаем пустую строку
        if (string.IsNullOrEmpty(htmlTemplate))
        {
            return string.Empty;
        }

        // Если модель данных null, возвращаем шаблон без изменений
        if (dataModel == null)
        {
            return htmlTemplate;
        }

        // Сначала обрабатываем циклы $foreach, чтобы создать контекст для каждого элемента
        string result = ProcessForeachBlocks(htmlTemplate, dataModel);

        // Потом обрабатываем условные конструкции $if/$else/$endif
        result = ProcessIfBlocks(result, dataModel);

        // В конце заменяем переменные ${Property} на значения из модели
        result = ProcessVariables(result, dataModel);

        // Возвращаем итоговый HTML
        return result;
    }

    /// <summary>
    /// Обрабатывает конструкции $foreach(var item in Collection) ... $endfor.
    /// Использует ручной парсер для корректной работы с вложенностью.
    /// </summary>
    /// <param name="template">HTML-шаблон с конструкциями $foreach</param>
    /// <param name="model">Объект модели данных</param>
    /// <returns>HTML-строка с развёрнутыми циклами</returns>
        private string ProcessForeachBlocks(string template, object model)
    {
        // Ищем первое вхождение конструкции $foreach с помощью регулярного выражения
        string foreachPattern = @"\$foreach\s*\(\s*var\s+(\w+)\s+in\s+(\w+(?:\.\w+)*)\s*\)";
        Match match = Regex.Match(template, foreachPattern);

        // Если конструкция $foreach не найдена, возвращаем шаблон без изменений
        if (!match.Success)
        {
            return template;
        }

        // Извлекаем имя переменной цикла из первой группы регулярного выражения (например, "user" или "item")
        string itemName = match.Groups[1].Value;

        // Извлекаем имя свойства-коллекции из второй группы регулярного выражения (например, "Users" или "Model.Users")
        string collectionName = match.Groups[2].Value;

        // Запоминаем индекс начала найденной конструкции $foreach в тексте шаблона
        int startIndex = match.Index;

        // Находим индекс парного закрывающего тега $endfor с учетом возможной вложенности других циклов
        int endIndex = FindMatchingEndFor(template, startIndex);

        // Если парный тег $endfor не найден, возвращаем шаблон без изменений во избежание ошибок парсинга
        if (endIndex == -1)
        {
            return template;
        }

        // Вырезаем внутренний шаблон цикла, находящийся между концом $foreach(...) и началом $endfor
        string innerTemplate = template.Substring(match.Index + match.Length, endIndex - match.Index - match.Length);

        // Получаем объект коллекции из переданной модели данных с помощью рефлексии по имени свойства
        object collectionObj = GetPropertyValue(model, collectionName);

        // Создаем экземпляр StringBuilder для эффективного и быстрого склеивания строк без создания лишних объектов в памяти
        StringBuilder result = new StringBuilder();

        // Добавляем в результат всю часть исходного шаблона, которая находится до начала конструкции $foreach
        result.Append(template.Substring(0, startIndex));

        // Проверяем, является ли полученный из модели объект перечисляемой коллекцией (например, списком или массивом)
        if (collectionObj is System.Collections.IEnumerable enumerable)
        {
            // Проходим по каждому отдельному элементу найденной коллекции
            foreach (object item in enumerable)
            {
                // Создаем словарь-обертку, где ключом является имя переменной цикла, а значением - текущий элемент
                // Это позволяет шаблонизатору корректно обращаться к свойствам элемента через имя переменной (например, ${user.Name})
                var wrapper = new System.Collections.Generic.Dictionary<string, object> { { itemName, item } };

                // Рекурсивно вызываем основной метод рендеринга для внутреннего шаблона, передавая словарь-обертку как модель данных
                string renderedItem = RenderFromString(innerTemplate, wrapper);

                // Добавляем обработанный фрагмент HTML для текущего элемента в общий результат
                result.Append(renderedItem);
            }
        }

        // Добавляем в результат всю часть исходного шаблона, которая находится сразу после закрывающего тега $endfor
        result.Append(template.Substring(endIndex + "$endfor".Length));

        // Рекурсивно вызываем этот же метод для обработанного результата, чтобы найти и развернуть следующие конструкции $foreach в шаблоне
        return ProcessForeachBlocks(result.ToString(), model);
    }

    private object GetPropertyValue(object model, string propertyName)
    {
        // Разделяем имя свойства по точкам для поддержки вложенных свойств (например, "User.Name" разобьется на "User" и "Name")
        string[] parts = propertyName.Split('.');

        // Инициализируем текущий объект для обхода цепочки свойств, начиная с переданной модели
        object current = model;

        // Проходим по каждой части имени свойства последовательно
        foreach (string part in parts)
        {
            // Если на каком-то этапе текущий объект оказался null, дальнейший поиск невозможен, возвращаем null
            if (current == null)
            {
                return null;
            }

            // Проверяем, является ли текущий объект словарем-оберткой, созданным внутри цикла $foreach
            if (current is System.Collections.Generic.IDictionary<string, object> dict && dict.TryGetValue(part, out var dictValue))
            {
                // Если ключ найден в словаре, присваиваем его значение текущему объекту для следующей итерации
                current = dictValue;
                // Переходим к следующей части имени свойства, пропуская стандартную рефлексию для этого шага
                continue;
            }

            // Получаем тип (структуру) текущего объекта во время выполнения программы
            Type type = current.GetType();

            // Ищем публичное свойство с заданным именем, игнорируя регистр букв (например, "name" найдет "Name")
            PropertyInfo property = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            // Если свойство с таким именем не найдено в типе объекта, возвращаем null
            if (property == null)
            {
                return null;
            }

            // Получаем фактическое значение свойства из текущего экземпляра объекта
            current = property.GetValue(current);
        }

        // Возвращаем итоговое значение, полученное после обхода всей цепочки вложенных свойств
        return current;
    }

    /// <summary>
    /// Находит позицию закрывающего $endfor с учётом вложенности.
    /// </summary>
    /// <param name="template">HTML-шаблон</param>
    /// <param name="startIndex">Позиция начала $foreach</param>
    /// <returns>Позиция конца $endfor или -1, если не найден</returns>
    private int FindMatchingEndFor(string template, int startIndex)
    {
        // Счётчик глубины вложенности
        int depth = 1;

        // Начинаем поиск после начала $foreach
        int searchIndex = startIndex + "$foreach".Length;

        // Ищем все вхождения $foreach и $endfor
        while (depth > 0)
        {
            // Ищем следующий $foreach
            int nextForeach = template.IndexOf("$foreach", searchIndex, StringComparison.OrdinalIgnoreCase);

            // Ищем следующий $endfor
            int nextEndFor = template.IndexOf("$endfor", searchIndex, StringComparison.OrdinalIgnoreCase);

            // Если не нашли ни то, ни другое — выходим
            if (nextForeach == -1 && nextEndFor == -1)
            {
                return -1;
            }

            // Если нашли $endfor раньше или только его
            if (nextEndFor != -1 && (nextForeach == -1 || nextEndFor < nextForeach))
            {
                // Уменьшаем глубину
                depth--;

                // Если глубина стала 0 — нашли нужный $endfor
                if (depth == 0)
                {
                    return nextEndFor;
                }

                // Продолжаем поиск после этого $endfor
                searchIndex = nextEndFor + "$endfor".Length;
            }
            // Если нашли $foreach раньше
            else if (nextForeach != -1)
            {
                // Увеличиваем глубину
                depth++;

                // Продолжаем поиск после этого $foreach
                searchIndex = nextForeach + "$foreach".Length;
            }
        }

        // Если дошли сюда — не нашли соответствующего $endfor
        return -1;
    }

    /// <summary>
    /// Обрабатывает конструкции $if(Condition) ... $else ... $endif.
    /// </summary>
    /// <param name="template">HTML-шаблон с конструкциями $if</param>
    /// <param name="model">Объект модели данных</param>
    /// <returns>HTML-строка с обработанными условиями</returns>
    private string ProcessIfBlocks(string template, object model)
    {
        // Ищем начало конструкции $if
        string ifPattern = @"\$if\s*\(\s*(\w+(?:\.\w+)*)\s*\)";
        Match match = Regex.Match(template, ifPattern);

        // Если не нашли $if, возвращаем шаблон без изменений
        if (!match.Success)
        {
            return template;
        }

        // Извлекаем имя свойства условия (например, "IsActive" или "User.IsActive")
        string conditionName = match.Groups[1].Value;

        // Находим конец блока $endif с учётом вложенности
        int startIndex = match.Index;
        int endIndex = FindMatchingEndIf(template, startIndex);

        // Если не нашли $endif, возвращаем шаблон без изменений
        if (endIndex == -1)
        {
            return template;
        }

        // Извлекаем содержимое блока $if (между $if и $endif)
        string blockContent = template.Substring(match.Index + match.Length, endIndex - match.Index - match.Length);

        // Ищем ветку $else внутри блока
        int elseIndex = blockContent.IndexOf("$else", StringComparison.OrdinalIgnoreCase);

        // Определяем содержимое веток $if и $else
        string ifContent;
        string elseContent;

        if (elseIndex != -1)
        {
            // Если есть $else, разделяем блок на две части
            ifContent = blockContent.Substring(0, elseIndex);
            elseContent = blockContent.Substring(elseIndex + "$else".Length);
        }
        else
        {
            // Если нет $else, весь блок — это ветка $if
            ifContent = blockContent;
            elseContent = string.Empty;
        }

        // Получаем значение условия из модели через рефлексию
        object conditionValue = GetPropertyValue(model, conditionName);

        // Проверяем, является ли значение истинным
        bool isTruthy = IsTruthy(conditionValue);

        // Создаём StringBuilder для результата
        StringBuilder result = new StringBuilder();

        // Добавляем часть шаблона до $if
        result.Append(template.Substring(0, startIndex));

        // Если условие истинно, добавляем содержимое ветки $if
        if (isTruthy)
        {
            result.Append(ifContent);
        }
        // Если условие ложно, добавляем содержимое ветки $else
        else
        {
            result.Append(elseContent);
        }

        // Добавляем часть шаблона после $endif
        result.Append(template.Substring(endIndex + "$endif".Length));

        // Рекурсивно обрабатываем результат (для вложенных $if)
        return ProcessIfBlocks(result.ToString(), model);
    }

    /// <summary>
    /// Находит позицию закрывающего $endif с учётом вложенности.
    /// </summary>
    /// <param name="template">HTML-шаблон</param>
    /// <param name="startIndex">Позиция начала $if</param>
    /// <returns>Позиция конца $endif или -1, если не найден</returns>
    private int FindMatchingEndIf(string template, int startIndex)
    {
        // Счётчик глубины вложенности
        int depth = 1;

        // Начинаем поиск после начала $if
        int searchIndex = startIndex + "$if".Length;

        // Ищем все вхождения $if и $endif
        while (depth > 0)
        {
            // Ищем следующий $if
            int nextIf = template.IndexOf("$if", searchIndex, StringComparison.OrdinalIgnoreCase);

            // Ищем следующий $endif
            int nextEndIf = template.IndexOf("$endif", searchIndex, StringComparison.OrdinalIgnoreCase);

            // Если не нашли ни то, ни другое — выходим
            if (nextIf == -1 && nextEndIf == -1)
            {
                return -1;
            }

            // Если нашли $endif раньше или только его
            if (nextEndIf != -1 && (nextIf == -1 || nextEndIf < nextIf))
            {
                // Уменьшаем глубину
                depth--;

                // Если глубина стала 0 — нашли нужный $endif
                if (depth == 0)
                {
                    return nextEndIf;
                }

                // Продолжаем поиск после этого $endif
                searchIndex = nextEndIf + "$endif".Length;
            }
            // Если нашли $if раньше
            else if (nextIf != -1)
            {
                // Увеличиваем глубину
                depth++;

                // Продолжаем поиск после этого $if
                searchIndex = nextIf + "$if".Length;
            }
        }

        // Если дошли сюда — не нашли соответствующего $endif
        return -1;
    }

    /// <summary>
    /// Заменяет конструкции ${Property} на значения из модели.
    /// Поддерживает вложенные свойства ${User.Name}.
    /// </summary>
    /// <param name="template">HTML-шаблон с переменными ${}</param>
    /// <param name="model">Объект модели данных</param>
    /// <returns>HTML-строка с подставленными значениями</returns>
    private string ProcessVariables(string template, object model)
    {
        // Регулярное выражение для поиска ${Property}
        string pattern = @"\$\{(\w+(?:\.\w+)*)\}";

        // Заменяем все вхождения ${Property} на значения из модели
        return Regex.Replace(template, pattern, match =>
        {
            // Извлекаем имя свойства (например, "Name" или "User.Name")
            string propertyName = match.Groups[1].Value;

            // Получаем значение свойства из модели через рефлексию
            object value = GetPropertyValue(model, propertyName);

            // Если значение null, возвращаем пустую строку
            if (value == null)
            {
                return string.Empty;
            }

            // HTML-кодируем значение для защиты от XSS
            string stringValue = value.ToString();
            return System.Net.WebUtility.HtmlEncode(stringValue);
        });
    }

    /// <summary>
    /// Получает значение свойства объекта через рефлексию.
    /// Поддерживает вложенные свойства (например, "User.Name").
    /// </summary>
    /// <param name="model">Объект модели данных</param>
    /// <param name="propertyName">Имя свойства (возможно с точками для вложенности)</param>
    /// <returns>Значение свойства или null, если не найдено</returns>

    /// <summary>
    /// Проверяет, является ли значение истинным для условий $if.
    /// </summary>
    /// <param name="value">Значение для проверки</param>
    /// <returns>True, если значение истинно; иначе false</returns>
    private bool IsTruthy(object value)
    {
        // Если значение null, считаем его ложным
        if (value == null)
        {
            return false;
        }

        // Если это строка, она истинна, только если не пустая
        if (value is string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        // Если это целое число, оно истинно, только если не равно нулю
        if (value is int i)
        {
            return i != 0;
        }

        // Если это число с плавающей точкой, оно истинно, только если не равно нулю
        if (value is double d)
        {
            return d != 0.0;
        }

        // Если это boolean, возвращаем его прямое логическое значение
        if (value is bool b)
        {
            return b;
        }

        // Для любых других объектов считаем их истинными
        return true;
    }

    /// <summary>
    /// Рендерит шаблон из файла, подставляя значения из модели данных.
    /// </summary>
    /// <param name="filePath">Путь к файлу шаблона</param>
    /// <param name="dataModel">Объект модели данных</param>
    /// <returns>Готовая HTML-строка</returns>
    public string RenderFromFile(string filePath, object dataModel)
    {
        // Читаем содержимое файла в кодировке UTF-8
        string template = File.ReadAllText(filePath, Encoding.UTF8);

        // Рендерим шаблон из строки
        return RenderFromString(template, dataModel);
    }

    /// <summary>
    /// Рендерит шаблон из файла и сохраняет результат в другой файл.
    /// </summary>
    /// <param name="inputFilePath">Путь к файлу шаблона</param>
    /// <param name="outputFilePath">Путь для сохранения результата</param>
    /// <param name="dataModel">Объект модели данных</param>
    /// <returns>Готовая HTML-строка</returns>
    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        // Рендерим шаблон из файла
        string result = RenderFromFile(inputFilePath, dataModel);

        // Записываем результат в выходной файл в кодировке UTF-8
        File.WriteAllText(outputFilePath, result, Encoding.UTF8);

        // Возвращаем итоговую строку
        return result;
    }
}