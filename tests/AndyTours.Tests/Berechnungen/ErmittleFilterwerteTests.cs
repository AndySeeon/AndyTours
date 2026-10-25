using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleFilterwerte"/>.
/// </summary>
public class ErmittleFilterwerteTests
{
    /// <summary>Datenbestand mit drei Equipments und vier Kategorien.</summary>
    private static Datenbestand Bestand() => new()
    {
        Equipments =
        {
            new Equipment { Id = 7, Name = "Genius" }, new Equipment { Id = 8, Name = "Rennrad" }, new Equipment { Id = 9, Name = "Altes Rad" },
        },
        Kategorien =
        {
            new Kategorie { Id = 1, Name = "Tour" }, new Kategorie { Id = 2, Name = "Alpen" },
            new Kategorie { Id = 3, Name = "arbeit" }, new Kategorie { Id = 4, Name = "Urlaub" },
        },
    };

    /// <summary>Einheit mit Equipment, Kategorien und Trainingsbereich.</summary>
    private static Trainingseinheit Einheit(long? equipment, long[] kategorien, string bereich) => new()
    {
        EquipmentId = equipment, KategorieIds = [.. kategorien], Trainingsbereich = bereich,
    };

    [Fact]
    public void Nur_Werte_der_Auswahl_alphabetisch_mit_alle_vorne()
    {
        Trainingseinheit[] auswahl =
        [
            Einheit(8, [1, 2], "GA2"),
            Einheit(7, [3, 1], "GA1"),
            Einheit(null, [], ""),
            Einheit(7, [2], "GA1"),
        ];

        Filterwerte werte = ErmittleFilterwerte.Ausfuehren(auswahl, Bestand(), Einheitenfilter.Leer);

        Assert.Equal(["(alle)", "Alpen", "arbeit", "Tour"], werte.Kategorien); // ohne „Urlaub“, Groß-/Kleinschreibung egal
        Assert.Equal(["(alle)", "Genius", "Rennrad"], werte.Equipments); // ohne „Altes Rad“ und ohne leeren Eintrag
        Assert.Equal(["(alle)", "GA1", "GA2"], werte.Trainingsbereiche);
    }

    [Fact]
    public void Ausgemustertes_Equipment_mit_Zusatz_am_Ende()
    {
        Datenbestand bestand = Bestand();
        bestand.Equipments[0].Ausgemustert = true; // Genius

        Filterwerte werte = ErmittleFilterwerte.Ausfuehren([Einheit(7, [1], ""), Einheit(8, [1], ""), Einheit(9, [1], "")], bestand, Einheitenfilter.Leer);

        Assert.Equal(["(alle)", "Altes Rad", "Rennrad", "Genius (ausgemustert)"], werte.Equipments);
    }

    [Fact]
    public void Leere_Auswahl_ergibt_nur_alle()
    {
        Filterwerte werte = ErmittleFilterwerte.Ausfuehren([], Bestand(), Einheitenfilter.Leer);

        Assert.Equal(["(alle)"], werte.Kategorien);
        Assert.Equal(["(alle)"], werte.Equipments);
        Assert.Equal(["(alle)"], werte.Trainingsbereiche);
    }

    [Fact]
    public void Unbekannte_Nummern_erscheinen_nicht()
    {
        Filterwerte werte = ErmittleFilterwerte.Ausfuehren([Einheit(99, [99], "GA1")], Bestand(), Einheitenfilter.Leer);

        Assert.Equal(["(alle)"], werte.Kategorien);
        Assert.Equal(["(alle)"], werte.Equipments);
    }

    [Fact]
    public void Gewaehlter_Wert_bleibt_auch_ohne_Vorkommen_in_der_Liste()
    {
        Einheitenfilter wahl = Einheitenfilter.Leer with { Kategorie = "Urlaub", Equipment = "Altes Rad", Trainingsbereich = "WSA" };

        Filterwerte werte = ErmittleFilterwerte.Ausfuehren([Einheit(7, [1], "GA1")], Bestand(), wahl);

        Assert.Equal(["(alle)", "Tour", "Urlaub"], werte.Kategorien);
        Assert.Equal(["(alle)", "Altes Rad", "Genius"], werte.Equipments);
        Assert.Equal(["(alle)", "GA1", "WSA"], werte.Trainingsbereiche);
    }

    [Fact]
    public void Gewaehlter_Wert_erscheint_nicht_doppelt()
    {
        Einheitenfilter wahl = Einheitenfilter.Leer with { Equipment = "Genius" };

        Filterwerte werte = ErmittleFilterwerte.Ausfuehren([Einheit(7, [1], "GA1")], Bestand(), wahl);

        Assert.Equal(["(alle)", "Genius"], werte.Equipments);
    }
}
