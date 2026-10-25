using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleTaetigkeiten"/>, <see cref="ZaehleTaetigkeit"/> und <see cref="BenenneTaetigkeitUm"/>.
/// </summary>
public class ZaehleUndBenenneTaetigkeitTests
{
    /// <summary>Wartungen mit „Kettenwechsel“ (zwei Schreibweisen), „Dämpfer eingestellt“ und ohne Tätigkeit.</summary>
    private static List<Wartung> Wartungen() =>
    [
        new() { Id = 1, Taetigkeit = "Kettenwechsel" },
        new() { Id = 2, Taetigkeit = "Dämpfer eingestellt" },
        new() { Id = 3, Taetigkeit = "kettenwechsel " },
        new() { Id = 4, Taetigkeit = " " },
        new() { Id = 5, Taetigkeit = "Bremsbelag HR" },
    ];

    [Fact]
    public void Anfangsliste_alphabetisch_ohne_Dubletten_und_Leere()
    {
        Assert.Equal(["Bremsbelag HR", "Dämpfer eingestellt", "Kettenwechsel"], ErmittleTaetigkeiten.Ausfuehren(Wartungen()));
    }

    [Theory]
    [InlineData("Kettenwechsel", 2)]
    [InlineData("KETTENWECHSEL", 2)]
    [InlineData("dämpfer eingestellt", 1)]
    [InlineData("Inspektion", 0)]
    public void Zaehlt_ohne_Unterschied_der_Schreibweise(string taetigkeit, int erwartet)
    {
        Assert.Equal(erwartet, ZaehleTaetigkeit.Ausfuehren(Wartungen(), taetigkeit));
    }

    [Fact]
    public void Umbenennen_aendert_alle_Schreibweisen()
    {
        List<Wartung> wartungen = Wartungen();

        int anzahl = BenenneTaetigkeitUm.Ausfuehren(wartungen, "Kettenwechsel", " Kette gewechselt ");

        Assert.Equal(2, anzahl);
        Assert.Equal(["Kette gewechselt", "Dämpfer eingestellt", "Kette gewechselt", " ", "Bremsbelag HR"],
            wartungen.Select(w => w.Taetigkeit));
    }
}
