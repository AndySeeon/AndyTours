using AndyTours.Core.Berechnungen;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="SaisonzielEingaben"/>.
/// </summary>
public class SaisonzielEingabenTests
{
    [Theory]
    [InlineData(0, "")]
    [InlineData(6000, "6000")]
    public void Zielwert_erscheint_als_Feldtext(int wert, string erwartet) =>
        Assert.Equal(erwartet, SaisonzielEingaben.AlsText(wert));

    [Fact]
    public void Gueltige_und_leere_Felder_ergeben_die_Werte()
    {
        var (werte, hinweis) = SaisonzielEingaben.Lesen("Bike", "6000", " ", "0");

        Assert.Null(hinweis);
        Assert.Equal((6000, 0, 0), werte);
    }

    [Theory]
    [InlineData("100000", true)]
    [InlineData("100001", false)]
    [InlineData("-1", false)]
    [InlineData("12,5", false)]
    [InlineData("viel", false)]
    public void Kilometer_werden_geprueft(string text, bool gueltig) =>
        Assert.Equal(gueltig ? null : "Kilometer bei ‚Bike‘: bitte eine ganze Zahl von 0 bis 100.000 eingeben.",
            SaisonzielEingaben.Lesen("Bike", text, "", "").Hinweis);

    [Theory]
    [InlineData("2000000", true)]
    [InlineData("2000001", false)]
    [InlineData("-5", false)]
    public void Hoehenmeter_werden_geprueft(string text, bool gueltig) =>
        Assert.Equal(gueltig ? null : "Höhenmeter bei ‚Bike‘: bitte eine ganze Zahl von 0 bis 2.000.000 eingeben.",
            SaisonzielEingaben.Lesen("Bike", "", text, "").Hinweis);

    [Theory]
    [InlineData("1000", true)]
    [InlineData("1001", false)]
    [InlineData("1,5", false)]
    public void Anzahl_wird_geprueft(string text, bool gueltig) =>
        Assert.Equal(gueltig ? null : "Anzahl bei ‚Bike‘: bitte eine ganze Zahl von 0 bis 1.000 eingeben.",
            SaisonzielEingaben.Lesen("Bike", "", "", text).Hinweis);

    [Fact]
    public void Der_Hinweis_nennt_den_ersten_Fehler()
    {
        string? hinweis = SaisonzielEingaben.Lesen("Laufen", "x", "y", "z").Hinweis;

        Assert.StartsWith("Kilometer bei ‚Laufen‘", hinweis);
    }
}
