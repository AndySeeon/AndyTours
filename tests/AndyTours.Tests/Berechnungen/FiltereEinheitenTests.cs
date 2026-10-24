using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="FiltereEinheiten"/>.
/// </summary>
public class FiltereEinheitenTests
{
    /// <summary>Datenbestand mit zwei Equipments und drei Kategorien.</summary>
    private static Datenbestand Bestand() => new()
    {
        Equipments = { new Equipment { Id = 7, Name = "Genius" }, new Equipment { Id = 8, Name = "Rennrad" } },
        Kategorien =
        {
            new Kategorie { Id = 1, Name = "Tour" }, new Kategorie { Id = 2, Name = "Alpen" }, new Kategorie { Id = 3, Name = "Arbeit" },
        },
    };

    /// <summary>Einheit mit den für den Filter wichtigen Werten.</summary>
    private static Trainingseinheit Einheit(long id, string name = "Tour", long? equipment = 7, long[]? kategorien = null,
        string bereich = "GA1", double km = 50, int hm = 500) => new()
    {
        Id = id, Name = name, EquipmentId = equipment, KategorieIds = [.. kategorien ?? [1]],
        Trainingsbereich = bereich, DistanzKm = km, Hoehenmeter = hm,
    };

    /// <summary>Kurzform: Nummern der gefilterten Einheiten.</summary>
    private static long[] Ids(IEnumerable<Trainingseinheit> einheiten, Einheitenfilter filter) =>
        FiltereEinheiten.Ausfuehren(einheiten, filter, Bestand()).Select(e => e.Id).ToArray();

    [Fact]
    public void Ohne_Filter_bleiben_alle_Einheiten_in_ihrer_Reihenfolge()
    {
        Assert.Equal([3, 1, 2], Ids([Einheit(3), Einheit(1), Einheit(2)], Einheitenfilter.Leer));
    }

    [Fact]
    public void Kategorie_passt_auch_bei_weiteren_Kategorien()
    {
        Trainingseinheit[] einheiten = [Einheit(1, kategorien: [1, 2]), Einheit(2, kategorien: [1]), Einheit(3, kategorien: [])];

        Assert.Equal([1], Ids(einheiten, Einheitenfilter.Leer with { Kategorie = "Alpen" }));
    }

    [Fact]
    public void Equipment_filtert_nach_Namen_und_schliesst_Einheiten_ohne_Equipment_aus()
    {
        Trainingseinheit[] einheiten = [Einheit(1, equipment: 7), Einheit(2, equipment: 8), Einheit(3, equipment: null)];

        Assert.Equal([2], Ids(einheiten, Einheitenfilter.Leer with { Equipment = "Rennrad" }));
    }

    [Fact]
    public void Unbekannter_Name_ergibt_keine_Einheiten()
    {
        Assert.Empty(Ids([Einheit(1)], Einheitenfilter.Leer with { Equipment = "Gibt es nicht" }));
        Assert.Empty(Ids([Einheit(1)], Einheitenfilter.Leer with { Kategorie = "Gibt es nicht" }));
    }

    [Fact]
    public void Trainingsbereich_filtert_nach_Kuerzel()
    {
        Trainingseinheit[] einheiten = [Einheit(1, bereich: "GA1"), Einheit(2, bereich: "GA2"), Einheit(3, bereich: "")];

        Assert.Equal([2], Ids(einheiten, Einheitenfilter.Leer with { Trainingsbereich = "GA2" }));
    }

    [Fact]
    public void Distanzbereich_schliesst_die_Grenzen_ein()
    {
        Trainingseinheit[] einheiten = [Einheit(1, km: 49.99), Einheit(2, km: 50), Einheit(3, km: 75), Einheit(4, km: 100), Einheit(5, km: 100.01)];

        Assert.Equal([2, 3, 4], Ids(einheiten, Einheitenfilter.Leer with { Distanz = new Filterbereich(50, 100) }));
        Assert.Equal([2, 3, 4, 5], Ids(einheiten, Einheitenfilter.Leer with { Distanz = new Filterbereich(50, null) }));
        Assert.Equal([1, 2], Ids(einheiten, Einheitenfilter.Leer with { Distanz = new Filterbereich(null, 50) }));
    }

    [Fact]
    public void Einheit_ohne_Distanz_faellt_bei_aktivem_Distanzfilter_heraus()
    {
        Trainingseinheit[] einheiten = [Einheit(1, km: 0), Einheit(2, km: 20)];

        Assert.Equal([2], Ids(einheiten, Einheitenfilter.Leer with { Distanz = new Filterbereich(null, 50) }));
        Assert.Equal([1, 2], Ids(einheiten, Einheitenfilter.Leer));
    }

    [Fact]
    public void Hoehenmeterbereich_schliesst_die_Grenzen_ein_und_null_hm_ist_ein_Wert()
    {
        Trainingseinheit[] einheiten = [Einheit(1, hm: 0), Einheit(2, hm: 500), Einheit(3, hm: 1000), Einheit(4, hm: 1001)];

        Assert.Equal([1, 2], Ids(einheiten, Einheitenfilter.Leer with { Hoehenmeter = new Filterbereich(0, 500) }));
        Assert.Equal([2, 3], Ids(einheiten, Einheitenfilter.Leer with { Hoehenmeter = new Filterbereich(500, 1000) }));
    }

    [Fact]
    public void Fehlerhafter_Bereich_schraenkt_nicht_ein()
    {
        Trainingseinheit[] einheiten = [Einheit(1, km: 10), Einheit(2, km: 0)];
        var fehlerhaft = new Filterbereich(100, 50, VonFehlerhaft: true, BisFehlerhaft: true);

        Assert.Equal([1, 2], Ids(einheiten, Einheitenfilter.Leer with { Distanz = fehlerhaft, Hoehenmeter = fehlerhaft }));
    }

    [Theory]
    [InlineData("alm")]
    [InlineData("ALM")]
    [InlineData("  Brü  ")]
    [InlineData("BRÜNDLING")]
    public void Name_enthaelt_den_Text_ohne_Ruecksicht_auf_Gross_und_Kleinschreibung(string text)
    {
        Trainingseinheit[] einheiten = [Einheit(1, name: "Bründlingalm"), Einheit(2, name: "Kampenwand")];

        Assert.Equal([1], Ids(einheiten, Einheitenfilter.Leer with { Name = text }));
    }

    [Fact]
    public void Name_nur_aus_Leerzeichen_filtert_nicht()
    {
        Assert.Equal([1], Ids([Einheit(1)], Einheitenfilter.Leer with { Name = "   " }));
    }

    [Fact]
    public void Mehrere_Filter_muessen_gleichzeitig_erfuellt_sein()
    {
        Trainingseinheit[] einheiten =
        [
            Einheit(1, name: "Alm", equipment: 7, kategorien: [2], km: 60),
            Einheit(2, name: "Alm", equipment: 8, kategorien: [2], km: 60), // falsches Equipment
            Einheit(3, name: "Alm", equipment: 7, kategorien: [1], km: 60), // falsche Kategorie
            Einheit(4, name: "Alm", equipment: 7, kategorien: [2], km: 30), // zu kurz
            Einheit(5, name: "See", equipment: 7, kategorien: [2], km: 60), // falscher Name
        ];
        Einheitenfilter filter = Einheitenfilter.Leer with
        {
            Kategorie = "Alpen", Equipment = "Genius", Distanz = new Filterbereich(50, null), Name = "alm",
        };

        Assert.Equal([1], Ids(einheiten, filter));
    }
}
