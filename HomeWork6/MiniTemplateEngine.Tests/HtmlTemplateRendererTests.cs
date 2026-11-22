using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace MiniTemplateEngine.Tests;

[TestClass]
public class HtmlTemplateRendererTests
{
    private HtmlTemplateRenderer _renderer;

    [TestInitialize]
    public void Setup()
    {
        _renderer = new HtmlTemplateRenderer();
    }

    [TestMethod]
    public void RenderFromString_SimpleVariable_ShouldReplace()
    {
        string template = "Привет, ${Name}!";
        var model = new { Name = "Айзат" };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("Привет, Айзат!", result);
    }

    [TestMethod]
    public void RenderFromString_MultipleVariables_ShouldReplaceAll()
    {
        string template = "${Greeting}, ${Name}! Тебе ${Age} лет.";
        var model = new { Greeting = "Привет", Name = "Айзат", Age = 19 };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("Привет, Айзат! Тебе 19 лет.", result);
    }

    [TestMethod]
    public void RenderFromString_NestedProperty_ShouldReplace()
    {
        string template = "${User.Name}";
        var model = new { User = new { Name = "Мария" } };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("Мария", result);
    }

    [TestMethod]
    public void RenderFromString_VariableNotFound_ShouldReturnEmpty()
    {
        string template = "Привет, ${Unknown}!";
        var model = new { Name = "Айзат" };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("Привет, !", result);
    }

    [TestMethod]
    public void RenderFromString_HtmlEncode_ShouldEscapeSpecialChars()
    {
        string template = "${Name}";
        var model = new { Name = "<script>alert(1)</script>" };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("&lt;script&gt;alert(1)&lt;/script&gt;", result);
    }

    [TestMethod]
    public void RenderFromString_EmptyTemplate_ShouldReturnEmpty()
    {
        string result = _renderer.RenderFromString("", new { Name = "Айзат" });

        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void RenderFromString_NullTemplate_ShouldReturnEmpty()
    {
        string result = _renderer.RenderFromString(null, new { Name = "Айзат" });

        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void RenderFromString_NullModel_ShouldReturnTemplate()
    {
        string template = "Привет, ${Name}!";

        string result = _renderer.RenderFromString(template, null);

        Assert.AreEqual(template, result);
    }

    [TestMethod]
    public void IfBlock_TrueBoolean_ShouldShowContent()
    {
        string template = "$if(Active)Да$endif";
        var model = new { Active = true };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("Да", result);
    }

    [TestMethod]
    public void IfBlock_FalseBoolean_ShouldHideContent()
    {
        string template = "$if(Active)Да$endif";
        var model = new { Active = false };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void IfBlock_WithElse_True_ShouldShowIfBranch()
    {
        string template = "$if(Active)Да$elseНет$endif";
        var model = new { Active = true };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("Да", result);
    }

    [TestMethod]
    public void IfBlock_WithElse_False_ShouldShowElseBranch()
    {
        string template = "$if(Active)Да$elseНет$endif";
        var model = new { Active = false };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("Нет", result);
    }

    [TestMethod]
    public void IfBlock_NullString_ShouldHideContent()
    {
        string template = "$if(Name)Имя есть$endif";
        var model = new { Name = (string)null };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void IfBlock_ZeroNumber_ShouldHideContent()
    {
        string template = "$if(Age)Возраст есть$endif";
        var model = new { Age = 0 };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void ForeachBlock_Strings_ShouldIterate()
    {
        string template = "$foreach(var item in Items)${item} $endfor";
        var model = new { Items = new[] { "A", "B", "C" } };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("A B C ", result);
    }

    [TestMethod]
    public void ForeachBlock_Objects_ShouldAccessProperties()
    {
        string template = "$foreach(var item in Grades)${item.Subject}:${item.Score} $endfor";
        var model = new
        {
            Grades = new[]
            {
                new { Subject = "C#", Score = 5 },
                new { Subject = "Алгоритмы", Score = 4 }
            }
        };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("C#:5 Алгоритмы:4 ", result);
    }

    [TestMethod]
    public void ForeachBlock_EmptyCollection_ShouldReturnEmpty()
    {
        string template = "$foreach(var item in Items)${item}$endfor";
        var model = new { Items = new string[] { } };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void Nested_IfInsideForeach_ShouldWork()
    {
        string template = "$foreach(var item in Items)$if(item.Show)${item.Name} $endif$endfor";
        var model = new
        {
            Items = new[]
            {
                new { Name = "A", Show = true },
                new { Name = "B", Show = false },
                new { Name = "C", Show = true }
            }
        };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("A C ", result);
    }

    [TestMethod]
    public void Nested_ForeachInsideIf_ShouldWork()
    {
        string template = "$if(Show)$foreach(var item in Items)${item} $endfor$endif";
        var model = new
        {
            Show = true,
            Items = new[] { "X", "Y" }
        };

        string result = _renderer.RenderFromString(template, model);

        Assert.AreEqual("X Y ", result);
    }

    [TestMethod]
    public void RenderFromFile_ExistingFile_ShouldRender()
    {
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "Привет, ${Name}!");
        var model = new { Name = "Айзат" };

        try
        {
            string result = _renderer.RenderFromFile(tempFile, model);
            Assert.AreEqual("Привет, Айзат!", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [TestMethod]
    public void RenderToFile_ShouldCreateFile()
    {
        string inputFile = Path.GetTempFileName();
        string outputFile = Path.GetTempFileName();
        File.WriteAllText(inputFile, "Привет, ${Name}!");
        var model = new { Name = "Айзат" };

        try
        {
            _renderer.RenderToFile(inputFile, outputFile, model);
            string result = File.ReadAllText(outputFile);
            Assert.AreEqual("Привет, Айзат!", result);
        }
        finally
        {
            File.Delete(inputFile);
            File.Delete(outputFile);
        }
    }
}