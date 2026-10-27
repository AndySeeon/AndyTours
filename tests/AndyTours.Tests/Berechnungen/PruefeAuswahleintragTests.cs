using AndyTours.Core.Berechnungen;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="PruefeAuswahleintrag"/>.
/// </summary>
public class PruefeAuswahleintragTests
{
    /// <summary>Beispielliste.</summary>
    private static readonly string[] Liste = ["K0", "GA1", "GA2"];

    [Theory]
    [InlineData("WSA")]
    [InlineData("  WSA ")]
    [InlineData("GA 2")]
    public void Neuer_Eintrag_ist_gueltig(string text)
    {
        Assert.Null(PruefeAuswahleintrag.Ausfuehren(Liste, text));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Leerer_Eintrag_wird_abgewiesen(string? text)
    {
        Assert.Equal("Bitte einen Eintrag eingeben.", PruefeAuswahleintrag.Ausfuehren(Liste, text));
    }

    [Theory]
    [InlineData("GA1")]
    [InlineData("ga1")]
    [InlineData(" Ga1  ")]
    public void Vorhandener_Eintrag_wird_ohne_Unterschied_der_Schreibweise_abgewiesen(string text)
    {
        Assert.Equal("‚GA1‘ steht bereits in der Liste.", PruefeAuswahleintrag.Ausfuehren(Liste, text));
    }

    [Fact]
    public void Umbenennen_nur_in_der_Schreibweise_ist_erlaubt()
    {
        Assert.Null(PruefeAuswahleintrag.Ausfuehren(Liste, "ga1", umbenannterIndex: 1));
    }

    [Fact]
    public void Umbenennen_in_einen_anderen_vorhandenen_Eintrag_wird_abgewiesen()
    {
        Assert.Equal("‚GA2‘ steht bereits in der Liste.", PruefeAuswahleintrag.Ausfuehren(Liste, "ga2", umbenannterIndex: 1));
    }

    [Fact]
    public void Umbenennen_in_einen_leeren_Eintrag_wird_abgewiesen()
    {
        Assert.Equal("Bitte einen Eintrag eingeben.", PruefeAuswahleintrag.Ausfuehren(Liste, " ", umbenannterIndex: 0));
    }
}
