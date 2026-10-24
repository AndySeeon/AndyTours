using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleSaisonziel"/>.
/// </summary>
public class ErmittleSaisonzielTests
{
    /// <summary>Aktivität mit Grundziel aus BikeXperience.</summary>
    private static readonly Aktivitaet Bike = new() { Id = 1, Name = "Bike", ZielKm = 6000.4, ZielHm = 80000, ZielAnzahl = 120 };

    /// <summary>Aktivität ohne Grundziel.</summary>
    private static readonly Aktivitaet Laufen = new() { Id = 2, Name = "Laufen" };

    /// <summary>Eigene Ziele: Bike ab 2024 und 2026 (2026 nur Anzahl, km und hm „kein Ziel“), Laufen ab 2025.</summary>
    private static readonly List<Saisonziel> Ziele =
    [
        new() { AktivitaetId = 1, AbJahr = 2024, ZielKm = 7000, ZielHm = 90000, ZielAnzahl = 130 },
        new() { AktivitaetId = 1, AbJahr = 2026, ZielKm = 0, ZielHm = 0, ZielAnzahl = 100 },
        new() { AktivitaetId = 2, AbJahr = 2025, ZielKm = 500 },
    ];

    [Fact]
    public void Eigenes_Ziel_des_Jahres_gilt_ohne_Kennzeichnung()
    {
        WirksamesSaisonziel ziel = ErmittleSaisonziel.Ausfuehren(Bike, Ziele, 2024);

        Assert.Equal(new WirksamesSaisonziel(7000, 90000, 130, true, null, false), ziel);
        Assert.Equal("", ziel.Herkunft);
    }

    [Fact]
    public void Ohne_eigenes_Ziel_gilt_das_naechstfruehere_Jahr()
    {
        WirksamesSaisonziel ziel = ErmittleSaisonziel.Ausfuehren(Bike, Ziele, 2025);

        Assert.Equal(new WirksamesSaisonziel(7000, 90000, 130, false, 2024, false), ziel);
        Assert.Equal("übernommen aus 2024", ziel.Herkunft);
    }

    [Fact]
    public void Null_verdeckt_ein_frueheres_Ziel()
    {
        WirksamesSaisonziel ziel = ErmittleSaisonziel.Ausfuehren(Bike, Ziele, 2027);

        Assert.Equal(new WirksamesSaisonziel(0, 0, 100, false, 2026, false), ziel);
    }

    [Fact]
    public void Vor_dem_ersten_eigenen_Ziel_gilt_das_Grundziel_aus_BikeXperience()
    {
        WirksamesSaisonziel ziel = ErmittleSaisonziel.Ausfuehren(Bike, Ziele, 2023);

        Assert.Equal(new WirksamesSaisonziel(6000, 80000, 120, false, null, true), ziel);
        Assert.Equal("übernommen aus BikeXperience", ziel.Herkunft);
    }

    [Fact]
    public void Ohne_eigenes_Ziel_und_Grundziel_gibt_es_kein_Ziel()
    {
        WirksamesSaisonziel ziel = ErmittleSaisonziel.Ausfuehren(Laufen, Ziele, 2024);

        Assert.Equal(new WirksamesSaisonziel(0, 0, 0, false, null, false), ziel);
        Assert.Equal("", ziel.Herkunft);
    }

    [Fact]
    public void Ziele_anderer_Aktivitaeten_zaehlen_nicht()
    {
        WirksamesSaisonziel ziel = ErmittleSaisonziel.Ausfuehren(Laufen, Ziele, 2026);

        Assert.Equal(new WirksamesSaisonziel(500, 0, 0, false, 2025, false), ziel);
    }
}
