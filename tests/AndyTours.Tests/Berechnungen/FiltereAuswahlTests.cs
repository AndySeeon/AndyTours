using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="FiltereAuswahl"/>.
/// </summary>
public class FiltereAuswahlTests
{
    /// <summary>Vier Einheiten: je zwei Jahre (2025, 2026) und zwei Aktivitäten (1, 2).</summary>
    private static readonly List<Trainingseinheit> Einheiten =
    [
        new() { Id = 1, Datum = new DateOnly(2025, 5, 1), AktivitaetId = 1 },
        new() { Id = 2, Datum = new DateOnly(2025, 7, 1), AktivitaetId = 2 },
        new() { Id = 3, Datum = new DateOnly(2026, 1, 1), AktivitaetId = 1 },
        new() { Id = 4, Datum = new DateOnly(2026, 12, 31), AktivitaetId = 1 },
    ];

    /// <summary>Filtert und liefert die Nummern der gefundenen Einheiten.</summary>
    private static IEnumerable<long> Ids(int? jahr, long? aktivitaetId) =>
        FiltereAuswahl.Ausfuehren(Einheiten, jahr, aktivitaetId).Select(e => e.Id);

    [Fact]
    public void Alle_Jahre_und_alle_Aktivitaeten_liefern_alles()
    {
        Assert.Equal([1, 2, 3, 4], Ids(null, null));
    }

    [Fact]
    public void Ein_Jahr_liefert_nur_dessen_Einheiten_vom_ersten_bis_zum_letzten_Tag()
    {
        Assert.Equal([3, 4], Ids(2026, null));
    }

    [Fact]
    public void Eine_Aktivitaet_liefert_nur_deren_Einheiten()
    {
        Assert.Equal([2], Ids(null, 2));
    }

    [Fact]
    public void Jahr_und_Aktivitaet_werden_kombiniert()
    {
        Assert.Equal([1], Ids(2025, 1));
    }

    [Fact]
    public void Auswahl_ohne_passende_Einheiten_ist_leer()
    {
        Assert.Empty(Ids(2026, 2));
    }
}
