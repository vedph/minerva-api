using System.Globalization;

namespace Minerva.Services.Test;

public class ItaTextQueryAssistantTest
{
    [Fact]
    public void ToQuery_VerbObjectValue()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery("trova la parola \"cane\"");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectValue()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery("parola \"cane\"");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectEqualsValue()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery("trova la parola uguale a \"cane\"");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectStartsWithValue()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery("trova la parola iniziante per \"cane\"");
        Assert.NotNull(query);
        Assert.Equal("[value^=\"cane\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectEndsWithValue()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery("trova la parola terminante per \"cane\"");
        Assert.NotNull(query);
        Assert.Equal("[value$=\"cane\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectContainsValue()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery("trova la parola contenente \"cane\"");
        Assert.NotNull(query);
        Assert.Equal("[value*=\"cane\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectSimilar0ToValue()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery("trova la parola poco simile a \"cane\"");
        Assert.NotNull(query);
        Assert.Equal("[value%=\"cane:" +
            $"{assistant.SimilarityThresholds[0].ToString(CultureInfo.InvariantCulture)}\"]",
            query);
    }

    [Fact]
    public void ToQuery_ObjectSimilar1ToValue()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery("trova la parola simile a \"cane\"");
        Assert.NotNull(query);
        Assert.Equal("[value%=\"cane:" +
            $"{assistant.SimilarityThresholds[1].ToString(CultureInfo.InvariantCulture)}\"]",
            query);
    }

    [Fact]
    public void ToQuery_ObjectSimilar2ToValue()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery("trova la parola molto simile a \"cane\"");
        Assert.NotNull(query);
        Assert.Equal("[value%=\"cane:" +
            $"{assistant.SimilarityThresholds[2].ToString(CultureInfo.InvariantCulture)}\"]",
            query);
    }

    [Fact]
    public void ToQuery_ObjectWildcardsValue()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery("trova la parola corrispondente a \"c?n?\"");
        Assert.NotNull(query);
        Assert.Equal($"[value?=\"c?n?\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectValueAsPosNoun()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery(
            "trova la parola \"cane\" come sostantivo");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"] AND [pos=\"NOUN\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectValueAsPosAdj()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery(
            "trova la parola \"brutto\" come aggettivo");
        Assert.NotNull(query);
        Assert.Equal("[value=\"brutto\"] AND [pos=\"ADJ\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectValueWithStyleBold()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery(
            "trova la parola \"cane\" in grassetto");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"] AND [b=\"1\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectValueWithStyleItalic()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery(
            "trova la parola \"cane\" in corsivo");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"] AND [i=\"1\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectValueWithStyleUnderline()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery(
            "trova la parola \"cane\" in sottolineato");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"] AND [u=\"1\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectValueAtSentenceStart()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery(
            "trova la parola \"cane\" a inizio di frase");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"] LALIGN(m=0) [$name=\"snt\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectValueNotAtSentenceStart()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery(
            "trova la parola \"cane\" non a inizio di frase");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"] LALIGN(n=1) [$name=\"snt\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectValueAtSentenceEnd()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery(
            "trova la parola \"cane\" a fine di frase");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"] RALIGN(m=0) [$name=\"snt\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectValueNotAtSentenceEnd()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery(
            "trova la parola \"cane\" non a fine di frase");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"] RALIGN(n=1) [$name=\"snt\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectValueBeforeObject()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery(
            "trova la parola \"cane\" prima della parola \"brutto\"");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"] BEFORE(m=3,s=snt) [value=\"brutto\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectValueAfterObject()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery(
            "trova la parola \"cane\" dopo la parola \"brutto\"");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"] AFTER(m=3,s=snt) [value=\"brutto\"]", query);
    }

    [Fact]
    public void ToQuery_ObjectValueNearObject()
    {
        ItaTextQueryAssistant assistant = new();
        string? query = assistant.ToQuery(
            "trova la parola \"cane\" vicino alla parola \"brutto\"");
        Assert.NotNull(query);
        Assert.Equal("[value=\"cane\"] NEAR(m=3,s=snt) [value=\"brutto\"]", query);
    }
}
