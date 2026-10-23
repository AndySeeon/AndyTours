using AndyTours.Core.Berechnungen;

namespace AndyTours.Tests.Datensicherung;

/// <summary>
/// Tests für <see cref="PruefeSicherungswert"/>.
/// </summary>
public class PruefeSicherungswertTests
{
    [Theory]
    [InlineData("1", 1)]
    [InlineData("99", 99)]
    [InlineData(" 7 ", 7)]
    [InlineData("03", 3)]
    public void Ganze_Zahlen_von_1_bis_99_sind_gueltig(string eingabe, int erwartet)
    {
        Assert.Equal(erwartet, PruefeSicherungswert.Ausfuehren(eingabe));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("100")]
    [InlineData("-1")]
    [InlineData("+5")]
    [InlineData("abc")]
    [InlineData("2,5")]
    [InlineData("2.5")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Andere_Eingaben_sind_ungueltig(string? eingabe)
    {
        Assert.Null(PruefeSicherungswert.Ausfuehren(eingabe));
    }
}
