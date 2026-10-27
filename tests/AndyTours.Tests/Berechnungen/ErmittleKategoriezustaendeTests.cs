using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleKategoriezustaende"/>.
/// </summary>
public class ErmittleKategoriezustaendeTests
{
    [Fact]
    public void Alle_keine_und_teilweise_alphabetisch()
    {
        // Einheiten 10 (Trainieren) und 11 (Trainieren, Training)
        Datenbestand bestand = Auswahlbestand.Erzeugen();
        List<Trainingseinheit> markiert = bestand.Trainingseinheiten.Where(e => e.Id is 10 or 11).ToList();

        List<(Kategorie Kategorie, bool? Zustand)> zustaende = ErmittleKategoriezustaende.Ausfuehren(bestand.Kategorien, markiert);

        Assert.Equal(["Leer", "Trainieren", "Training", "Urlaub"], zustaende.Select(z => z.Kategorie.Name));
        Assert.Equal([false, true, null, false], zustaende.Select(z => z.Zustand));
    }

    [Fact]
    public void Eine_Einheit_kennt_kein_teilweise()
    {
        Datenbestand bestand = Auswahlbestand.Erzeugen();

        List<(Kategorie Kategorie, bool? Zustand)> zustaende =
            ErmittleKategoriezustaende.Ausfuehren(bestand.Kategorien, [bestand.Trainingseinheiten.Single(e => e.Id == 11)]);

        Assert.DoesNotContain(zustaende, z => z.Zustand is null);
    }

    [Fact]
    public void Ohne_Einheiten_gilt_jede_Kategorie_als_keine()
    {
        Assert.All(ErmittleKategoriezustaende.Ausfuehren(Auswahlbestand.Erzeugen().Kategorien, []), z => Assert.False(z.Zustand));
    }
}
