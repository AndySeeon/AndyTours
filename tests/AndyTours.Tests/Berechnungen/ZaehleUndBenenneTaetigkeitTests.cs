using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleTaetigkeiten"/>, <see cref="ZaehleTaetigkeit"/> und <see cref="BenenneTaetigkeitUm"/>.
/// </summary>
public class ZaehleUndBenenneTaetigkeitTests
{
    /// <summary>
    /// Wartungen mit „Kettenwechsel“ (zwei Schreibweisen), „Dämpfer eingestellt“ und ohne Tätigkeit, dazu ein Alarm
    /// „Kettenwechsel“.
    /// </summary>
    private static List<Wartung> Wartungen() =>
    [
        new() { Id = 1, Taetigkeit = "Kettenwechsel" },
        new() { Id = 2, Taetigkeit = "Dämpfer eingestellt" },
        new() { Id = 3, Taetigkeit = "kettenwechsel " },
        new() { Id = 4, Taetigkeit = " " },
        new() { Id = 5, Taetigkeit = "Bremsbelag HR" },
        new() { Id = 6, Taetigkeit = "Kettenwechsel", Typ = WartungsTyp.Alarm, AlarmKm = 3000 },
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
        Assert.Equal(erwartet, ZaehleTaetigkeit.Ausfuehren(Wartungen(), taetigkeit, WartungsTyp.Wartung));
    }

    [Fact]
    public void Alarme_werden_getrennt_gezaehlt()
    {
        Assert.Equal(1, ZaehleTaetigkeit.Ausfuehren(Wartungen(), "kettenwechsel", WartungsTyp.Alarm));
        Assert.Equal(0, ZaehleTaetigkeit.Ausfuehren(Wartungen(), "Bremsbelag HR", WartungsTyp.Alarm));
    }

    [Fact]
    public void Umbenennen_aendert_alle_Schreibweisen_auch_im_Alarm()
    {
        List<Wartung> wartungen = Wartungen();

        int anzahl = BenenneTaetigkeitUm.Ausfuehren(wartungen, "Kettenwechsel", " Kette gewechselt ");

        Assert.Equal(3, anzahl);
        Assert.Equal(["Kette gewechselt", "Dämpfer eingestellt", "Kette gewechselt", " ", "Bremsbelag HR", "Kette gewechselt"],
            wartungen.Select(w => w.Taetigkeit));
    }
}
