using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ZaehleTrainingsbereich"/> und <see cref="BenenneTrainingsbereichUm"/>.
/// </summary>
public class ZaehleUndBenenneTrainingsbereichTests
{
    /// <summary>Einheiten mit GA2 (zwei Schreibweisen), GA1 und ohne Bereich.</summary>
    private static List<Trainingseinheit> Einheiten() =>
    [
        new() { Id = 1, Trainingsbereich = "GA2", Name = "Eins" },
        new() { Id = 2, Trainingsbereich = "GA1" },
        new() { Id = 3, Trainingsbereich = "ga2 " },
        new() { Id = 4, Trainingsbereich = "" },
    ];

    [Theory]
    [InlineData("GA2", 2)]
    [InlineData("ga2", 2)]
    [InlineData("GA1", 1)]
    [InlineData("WSA", 0)]
    public void Zaehlt_ohne_Unterschied_der_Schreibweise(string bereich, int erwartet)
    {
        Assert.Equal(erwartet, ZaehleTrainingsbereich.Ausfuehren(Einheiten(), bereich));
    }

    [Fact]
    public void Umbenennen_aendert_alle_Schreibweisen_und_sonst_nichts()
    {
        List<Trainingseinheit> einheiten = Einheiten();

        int anzahl = BenenneTrainingsbereichUm.Ausfuehren(einheiten, "GA2", "GA 2");

        Assert.Equal(2, anzahl);
        Assert.Equal(["GA 2", "GA1", "GA 2", ""], einheiten.Select(e => e.Trainingsbereich));
        Assert.Equal("Eins", einheiten[0].Name);
    }

    [Fact]
    public void Umbenennen_eines_unbenutzten_Bereichs_aendert_keine_Einheit()
    {
        List<Trainingseinheit> einheiten = Einheiten();

        Assert.Equal(0, BenenneTrainingsbereichUm.Ausfuehren(einheiten, "WSA", "SK"));
        Assert.Equal(["GA2", "GA1", "ga2 ", ""], einheiten.Select(e => e.Trainingsbereich));
    }
}
