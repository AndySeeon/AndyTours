using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="PasstZumSuchbegriff"/>.
/// </summary>
public class PasstZumSuchbegriffTests
{
    /// <summary>Einheit „Bründlingalm“ mit dem Kommentar „mit Sabine“.</summary>
    private static Trainingseinheit Einheit() => new() { Name = "Bründlingalm", Kommentar = "mit Sabine" };

    [Theory]
    [InlineData("alm")]          // im Namen, Teil eines Worts
    [InlineData("BRÜNDLING")]    // Groß- und Kleinschreibung zählt nicht
    [InlineData("sabine")]       // im Kommentar
    [InlineData("  Sab  ")]      // Leerzeichen am Rand zählen nicht
    public void Ein_Wort_in_Name_oder_Kommentar_passt(string suchbegriff)
    {
        Assert.True(PasstZumSuchbegriff.Ausfuehren(Einheit(), suchbegriff));
    }

    [Theory]
    [InlineData("Sabine Alm")]
    [InlineData("Alm Sabine")]
    [InlineData("mit   alm")]    // mehrere Leerzeichen zwischen den Wörtern
    public void Mehrere_Woerter_in_beliebiger_Reihenfolge(string suchbegriff)
    {
        Assert.True(PasstZumSuchbegriff.Ausfuehren(Einheit(), suchbegriff));
    }

    [Theory]
    [InlineData("Kampenwand")]
    [InlineData("Sabine Kampenwand")]  // ein Wort fehlt
    public void Fehlt_ein_Wort_passt_die_Einheit_nicht(string suchbegriff)
    {
        Assert.False(PasstZumSuchbegriff.Ausfuehren(Einheit(), suchbegriff));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Ohne_Woerter_passt_jede_Einheit(string suchbegriff)
    {
        Assert.True(PasstZumSuchbegriff.Ausfuehren(new Trainingseinheit { Name = "Tour" }, suchbegriff));
    }

    [Fact]
    public void Ohne_Kommentar_zaehlt_nur_der_Name()
    {
        var einheit = new Trainingseinheit { Name = "Kampenwand" };

        Assert.True(PasstZumSuchbegriff.Ausfuehren(einheit, "kampen"));
        Assert.False(PasstZumSuchbegriff.Ausfuehren(einheit, "Sabine"));
    }
}
